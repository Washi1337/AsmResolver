using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.DotNet.TestCases.Types.Structs;
using AsmResolver.PE.DotNet.Cil;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class FieldDefinitionTest
    {
        private FieldDefinition RebuildAndLookup(FieldDefinition field)
        {
            var stream = new MemoryStream();
            field.Module.Write(stream);
            
            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            return newModule
                .TopLevelTypes.First(t => t.FullName == field.DeclaringType.FullName)
                .Fields.First(f => f.Name == field.Name);
        }
        
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            Assert.Equal(nameof(SingleField.IntField), type.Fields[0].Name);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "NewName";
            
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            var field = type.Fields[0];
            
            type.Fields[0].Name = newName;
            
            var newField = RebuildAndLookup(field);
            Assert.Equal(newName, newField.Name);
        }

        [Fact]
        public void ReadDeclaringType()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var field = (FieldDefinition) module.LookupMember(
                typeof(SingleField).GetField(nameof(SingleField.IntField)).MetadataToken);
            Assert.NotNull(field.DeclaringType);
            Assert.Equal(nameof(SingleField), field.DeclaringType.Name);
        }

        [Fact]
        public void ReadFieldSignature()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var field = (FieldDefinition) module.LookupMember(
                typeof(SingleField).GetField(nameof(SingleField.IntField)).MetadataToken);
            Assert.NotNull(field.Signature);
            Assert.True(field.Signature.FieldType.IsTypeOf("System", "Int32"), "Field type should be System.Int32");
        }

        [Fact]
        public void PersistentFieldSignature()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var field = (FieldDefinition) module.LookupMember(
                typeof(SingleField).GetField(nameof(SingleField.IntField)).MetadataToken);

            field.Signature = FieldSignature.CreateInstance(module.CorLibTypeFactory.Byte);

            var newField = RebuildAndLookup(field);
            
            Assert.True(newField.Signature.FieldType.IsTypeOf("System", "Byte"), "Field type should be System.Byte");
        }

        [Fact]
        public void ReadFieldRva()
        {
            var module = ModuleDefinition.FromFile(typeof(InitialValues).Assembly.Location);
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
            var module = ModuleDefinition.FromFile(typeof(InitialValues).Assembly.Location);
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

        [Theory]
        [InlineData(nameof(ExplicitOffsetsStruct.IntField), 0)]
        [InlineData(nameof(ExplicitOffsetsStruct.ByteField), 10)]
        [InlineData(nameof(ExplicitOffsetsStruct.BoolField), 100)]
        public void ReadFieldOffset(string name, int offset)
        {
            var module = ModuleDefinition.FromFile(typeof(ExplicitOffsetsStruct).Assembly.Location);
            var field = module
                .TopLevelTypes.First(t => t.Name == nameof(ExplicitOffsetsStruct))
                .Fields.First(f => f.Name == name);

            Assert.Equal(offset, field.FieldOffset);
        }
        
    }
}