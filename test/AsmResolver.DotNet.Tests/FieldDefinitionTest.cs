using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.DotNet.TestCases.Types.Structs;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class FieldDefinitionTest
    {
        private static FieldDefinition RebuildAndLookup(FieldDefinition field)
        {
            var stream = new MemoryStream();
            field.DeclaringModule!.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray(), TestReaderParameters);
            return newModule
                .TopLevelTypes.First(t => t.FullName == field.DeclaringType!.FullName)
                .Fields.First(f => f.Name == field.Name);
        }

        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            Assert.Equal(nameof(SingleField.IntField), type.Fields[0].Name);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "NewName";

            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            var field = type.Fields[0];

            type.Fields[0].Name = newName;

            var newField = RebuildAndLookup(field);
            Assert.Equal(newName, newField.Name);
        }

        [Fact]
        public void ReadDeclaringType()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location, TestReaderParameters);
            var field = (FieldDefinition) module.LookupMember(
                typeof(SingleField).GetField(nameof(SingleField.IntField)).MetadataToken);
            Assert.NotNull(field.DeclaringType);
            Assert.Equal(nameof(SingleField), field.DeclaringType.Name);
        }

        [Fact]
        public void ReadFieldSignature()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location, TestReaderParameters);
            var field = (FieldDefinition) module.LookupMember(
                typeof(SingleField).GetField(nameof(SingleField.IntField)).MetadataToken);
            Assert.NotNull(field.Signature);
            Assert.True(field.Signature.FieldType.IsTypeOf("System", "Int32"), "Field type should be System.Int32");
        }

        [Fact]
        public void PersistentFieldSignature()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location, TestReaderParameters);
            var field = (FieldDefinition) module.LookupMember(
                typeof(SingleField).GetField(nameof(SingleField.IntField)).MetadataToken);

            field.Signature = new FieldSignature(module.CorLibTypeFactory.Byte);

            var newField = RebuildAndLookup(field);

            Assert.True(newField.Signature.FieldType.IsTypeOf("System", "Byte"), "Field type should be System.Byte");
        }

        [Fact]
        public void ReadFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location, TestReaderParameters);
            var field = (FieldDefinition) module.LookupMember(
                typeof(SingleField).GetField(nameof(SingleField.IntField)).MetadataToken);

            Assert.Equal("System.Int32 AsmResolver.DotNet.TestCases.Fields.SingleField::IntField", field.FullName);
        }

        [Fact]
        public void ReadFieldRva()
        {
            var module = ModuleDefinition.FromFile(typeof(InitialValues).Assembly.Location, TestReaderParameters);
            var field = module
                .TopLevelTypes.First(t => t.Name == nameof(InitialValues))
                .Fields.First(f => f.Name == nameof(InitialValues.ByteArray));

            var initializer = field.FindInitializerField();
            Assert.NotNull(initializer.FieldRva);
            Assert.IsAssignableFrom<IReadableSegment>(initializer.FieldRva);

            Assert.Equal(InitialValues.ByteArray, ((IReadableSegment) initializer.FieldRva).ToArray());
        }

        [Fact]
        public void PersistentFieldRva()
        {
            var module = ModuleDefinition.FromFile(typeof(InitialValues).Assembly.Location, TestReaderParameters);
            var field = module
                .TopLevelTypes.First(t => t.Name == nameof(InitialValues))
                .Fields.First(f => f.Name == nameof(InitialValues.ByteArray));

            var initializer = field.FindInitializerField();

            var data = new byte[]
            {
                1, 2, 3, 4
            };
            initializer.FieldRva = new DataSegment(data);
            initializer.Signature.FieldType.Resolve().ClassLayout.ClassSize = (uint) data.Length;

            var newInitializer = RebuildAndLookup(initializer);

            Assert.NotNull(newInitializer.FieldRva);
            Assert.IsAssignableFrom<IReadableSegment>(newInitializer.FieldRva);
            Assert.Equal(data, ((IReadableSegment) newInitializer.FieldRva).ToArray());
        }

        [Fact]
        public void ReadInvalidFieldRva()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.FieldRvaTest, TestReaderParameters);
            Assert.Throws<NotSupportedException>(() =>
                module.GetModuleType()!.Fields.First(f => f.Name == "InvalidFieldRva").FieldRva);

            module = ModuleDefinition.FromBytes(Properties.Resources.FieldRvaTest,
                new ModuleReaderParameters(EmptyErrorListener.Instance));
            Assert.Null(module.GetModuleType()!.Fields.First(f => f.Name == "InvalidFieldRva").FieldRva);
        }

        [Fact]
        public void ReadVirtualFieldRva()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_VirtualSegment, TestReaderParameters);
            var data = module.GetModuleType()!.Fields.First(f => f.Name == "__dummy__").FieldRva;
            var readableData = Assert.IsAssignableFrom<IReadableSegment>(data);
            Assert.Equal(new byte[4], readableData.ToArray());
            Assert.Equal(new byte[4], data.WriteIntoArray());
        }

        [Fact]
        public void ReadNativeIntFieldRvaX86()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_IntPtrFieldRva_X86, TestReaderParameters);
            var data = module.GetModuleType()!.Fields.First(f => f.Name == "__dummy__").FieldRva;
            var readableData = Assert.IsAssignableFrom<IReadableSegment>(data);
            Assert.Equal(new byte[] {0xEF, 0xCD, 0xAB, 0x89}, readableData.ToArray());
        }

        [Fact]
        public void ReadNativeIntFieldRvaX64()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_IntPtrFieldRva_X64, TestReaderParameters);
            var data = module.GetModuleType()!.Fields.First(f => f.Name == "__dummy__").FieldRva;
            var readableData = Assert.IsAssignableFrom<IReadableSegment>(data);
            Assert.Equal(new byte[] {0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01}, readableData.ToArray());
        }

        [Theory]
        [InlineData(nameof(ExplicitOffsetsStruct.IntField), 0)]
        [InlineData(nameof(ExplicitOffsetsStruct.ByteField), 10)]
        [InlineData(nameof(ExplicitOffsetsStruct.BoolField), 100)]
        public void ReadFieldOffset(string name, int offset)
        {
            var module = ModuleDefinition.FromFile(typeof(ExplicitOffsetsStruct).Assembly.Location, TestReaderParameters);
            var field = module
                .TopLevelTypes.First(t => t.Name == nameof(ExplicitOffsetsStruct))
                .Fields.First(f => f.Name == name);

            Assert.Equal(offset, field.FieldOffset);
        }

        [Theory]
        [InlineData(nameof(ExplicitOffsetsStruct.IntField), 0)]
        [InlineData(nameof(ExplicitOffsetsStruct.ByteField), 10)]
        [InlineData(nameof(ExplicitOffsetsStruct.BoolField), 100)]
        public void PersistentFieldOffset(string name, int offset)
        {
            var module = ModuleDefinition.FromFile(typeof(ExplicitOffsetsStruct).Assembly.Location, TestReaderParameters);
            var field = module
                .TopLevelTypes.First(t => t.Name == nameof(ExplicitOffsetsStruct))
                .Fields.First(f => f.Name == name);
            var newField = RebuildAndLookup(field);

            Assert.Equal(offset, newField.FieldOffset);
        }

        [Fact]
        public void AddSameFieldTwiceToTypeShouldThrow()
        {
            var module = new ModuleDefinition("SomeModule");
            var field = new FieldDefinition("SomeField", FieldAttributes.Public, module.CorLibTypeFactory.Int32);
            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public);
            type.Fields.Add(field);
            Assert.Throws<ArgumentException>(() => type.Fields.Add(field));
        }

        [Fact]
        public void AddSameFieldToDifferentTypesShouldThrow()
        {
            var module = new ModuleDefinition("SomeModule");
            var field = new FieldDefinition("SomeField", FieldAttributes.Public, module.CorLibTypeFactory.Int32);
            var type1 = new TypeDefinition("SomeNamespace", "SomeType1", TypeAttributes.Public);
            var type2 = new TypeDefinition("SomeNamespace", "SomeType2", TypeAttributes.Public);
            type1.Fields.Add(field);
            Assert.Throws<ArgumentException>(() => type2.Fields.Add(field));
        }

        [Fact]
        public void ExternalTypeDefAsFieldTypeShouldAutoConvertToTypeRef()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            var sourceType = sourceModule.LookupMember<TypeDefinition>(typeof(TopLevelClass1).MetadataToken);

            var targetModule = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var field = new FieldDefinition("Field", FieldAttributes.Static, sourceType.ToTypeSignature());
            targetModule.GetOrCreateModuleType().Fields.Add(field);

            var newField = RebuildAndLookup(field);

            var newType = Assert.IsAssignableFrom<TypeReference>(newField.Signature?.FieldType.GetUnderlyingTypeDefOrRef());
            Assert.Equal<ITypeDefOrRef>(sourceType, newType, SignatureComparer.Default);
        }

        [Fact]
        public void ExternalTypeDefAsGenericFieldTypeArgumentShouldAutoConvertToTypeRef()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            var sourceType = sourceModule.LookupMember<TypeDefinition>(typeof(TopLevelClass1).MetadataToken);

            var targetModule = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var field = new FieldDefinition("Field", FieldAttributes.Static,
                targetModule.CorLibTypeFactory.CorLibScope
                    .CreateTypeReference("System", "Action`1")
                    .MakeGenericInstanceType(sourceType.ToTypeSignature())
            );
            targetModule.GetOrCreateModuleType().Fields.Add(field);

            var newField = RebuildAndLookup(field);

            var newGenericType = Assert.IsAssignableFrom<GenericInstanceTypeSignature>(newField.Signature?.FieldType);
            var newType = Assert.IsAssignableFrom<TypeReference>(newGenericType.TypeArguments[0].GetUnderlyingTypeDefOrRef());
            Assert.Equal<ITypeDefOrRef>(sourceType, newType, SignatureComparer.Default);
        }
    }
}
