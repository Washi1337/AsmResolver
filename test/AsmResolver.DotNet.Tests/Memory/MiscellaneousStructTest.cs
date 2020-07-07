using System.Runtime.CompilerServices;
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

            Assert.Throws<CyclicStructureException>(() => struct1.GetImpliedMemoryLayout(false));
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

            var layout = type.GetImpliedMemoryLayout(false);
            Assert.Equal((uint) Unsafe.SizeOf<StructWithStaticField>(), layout.Size);
        }

    }
}