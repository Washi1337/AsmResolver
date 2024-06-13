using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Memory;
using AsmResolver.PE.DotNet.Metadata.Tables;
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
            var module = ModuleDefinition.FromFile(typeof(MiscellaneousStructTest).Assembly.Location, TestReaderParameters);
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
            var module = ModuleDefinition.FromFile(typeof(MiscellaneousStructTest).Assembly.Location, TestReaderParameters);
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
            var module = ModuleDefinition.FromFile(typeof(MiscellaneousStructTest).Assembly.Location, TestReaderParameters);
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
            var module = ModuleDefinition.FromFile(typeof(MiscellaneousStructTest).Assembly.Location, TestReaderParameters);
            var type = (TypeDefinition) module.LookupMember(typeof(NestedPlatformDependentStruct).MetadataToken);

            var layout = type.GetImpliedMemoryLayout(IntPtr.Size == 4);
            Assert.Equal(IntPtr.Size == 4, layout.Is32Bit);
            Assert.True(layout.IsPlatformDependent);
            Assert.Equal((uint) Unsafe.SizeOf<NestedPlatformDependentStruct>(), layout.Size);
        }

        private struct ManagedStruct
        {
            public string ManagedField;
        }

        [Theory]
        [InlineData(typeof(SequentialTestStructs.EmptyStruct), false)]
        [InlineData(typeof(Struct1), false)]
        [InlineData(typeof(NestedPlatformDependentStruct), false)]
        [InlineData(typeof(MiscellaneousStructTest), true)]
        [InlineData(typeof(ManagedStruct), true)]
        public void DetermineNonGenericIsReferenceOrContainsReferences(Type type, bool expected)
        {
            var module = ModuleDefinition.FromFile(type.Assembly.Location, TestReaderParameters);
            var t = module.LookupMember<TypeDefinition>(type.MetadataToken);

            var layout = t.GetImpliedMemoryLayout(false);
            Assert.Equal(expected, layout.IsReferenceOrContainsReferences);
        }

        [Theory]
        [InlineData(ElementType.I4, false)]
        [InlineData(ElementType.String, true)]
        public void DetermineGenericIsReferenceOrContainsReferences(ElementType elementType, bool expected)
        {
            var type = typeof(SequentialTestStructs.GenericStruct<,>);
            var module = ModuleDefinition.FromFile(type.Assembly.Location, TestReaderParameters);

            var paramType = module.CorLibTypeFactory.FromElementType(elementType)!;
            var t = module.LookupMember<TypeDefinition>(type.MetadataToken)
                .MakeGenericInstanceType(paramType, paramType);

            var layout = t.GetImpliedMemoryLayout(false);
            Assert.Equal(expected, layout.IsReferenceOrContainsReferences);
        }
    }
}
