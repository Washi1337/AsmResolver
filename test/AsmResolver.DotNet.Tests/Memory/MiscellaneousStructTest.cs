using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Memory;
using Xunit;

// Ignore unused field warnings.
#pragma warning disable 649

namespace AsmResolver.DotNet.Tests.Memory
{
    public class MiscellaneousStructTest
    {
        private struct Struct1
        {
            public Struct2 Field1;
        }

        private struct Struct2
        {
            public int Field2;
        }

        [Fact]
        public void CyclicDependencyTest()
        {
            var module = ModuleDefinition.FromFile(typeof(MiscellaneousStructTest).Assembly.Location);
            var struct1 = (TypeDefinition) module.LookupMember(typeof(Struct1).MetadataToken);
            var struct2 = (TypeDefinition) module.LookupMember(typeof(Struct2).MetadataToken);

            struct2.Fields[0].Signature.FieldType = struct1.ToTypeSignature();

            Assert.Throws<CyclicStructureException>(() => struct1.GetImpliedMemoryLayout(IntPtr.Size == 4));
        }

        private struct StructWithStaticField
        {
            public int Field1;

            public static int StaticField1;
        }

        [Fact]
        public void DetermineLayoutOfStructWithStaticFieldsShouldIgnoreStaticFields()
        {
            var module = ModuleDefinition.FromFile(typeof(MiscellaneousStructTest).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(StructWithStaticField).MetadataToken);

            var layout = type.GetImpliedMemoryLayout(IntPtr.Size == 4);
            Assert.Equal((uint) Unsafe.SizeOf<StructWithStaticField>(), layout.Size);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PlatformDependentStruct
        {
            public IntPtr Field1;
            public IntPtr Field2;
        }

        [Fact]
        public void DeterminePlatformDependentSize()
        {
            var module = ModuleDefinition.FromFile(typeof(MiscellaneousStructTest).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(PlatformDependentStruct).MetadataToken);

            var layout = type.GetImpliedMemoryLayout(IntPtr.Size == 4);
            Assert.Equal(IntPtr.Size == 4, layout.Is32Bit);
            Assert.True(layout.IsPlatformDependent);
            Assert.Equal((uint) Unsafe.SizeOf<PlatformDependentStruct>(), layout.Size);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NestedPlatformDependentStruct
        {
            public PlatformDependentStruct Struct1;
        }

        [Fact]
        public void DetermineNestedPlatformDependentSize()
        {
            var module = ModuleDefinition.FromFile(typeof(MiscellaneousStructTest).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(NestedPlatformDependentStruct).MetadataToken);

            var layout = type.GetImpliedMemoryLayout(IntPtr.Size == 4);
            Assert.Equal(IntPtr.Size == 4, layout.Is32Bit);
            Assert.True(layout.IsPlatformDependent);
            Assert.Equal((uint) Unsafe.SizeOf<NestedPlatformDependentStruct>(), layout.Size);
        }
    }
}
