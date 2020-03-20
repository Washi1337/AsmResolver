using System.IO;
using System.Linq;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
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

        private static FieldDefinition FindInitializerField(FieldDefinition field)
        {
            var cctor = field.DeclaringType.GetStaticConstructor();
            
            var instructions = cctor.CilMethodBody.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].OpCode.Code == CilCode.Ldtoken
                    && instructions[i + 2].OpCode.Code == CilCode.Stsfld
                    && instructions[i+2].Operand is FieldDefinition f
                    && f == field)
                {
                    return (FieldDefinition) instructions[i].Operand;
                }
            }

            return null;
        }

        [Fact]
        public void ReadFieldRva()
        {
            var module = ModuleDefinition.FromFile(typeof(InitialValues).Assembly.Location);
            var field = module
                .TopLevelTypes.First(t => t.Name == nameof(InitialValues))
                .Fields.First(f => f.Name == nameof(InitialValues.ByteArray));

            var initializer = FindInitializerField(field);
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

            var initializer = FindInitializerField(field);
            
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
    }
}