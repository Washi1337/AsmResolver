using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Properties;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class PropertyDefinitionTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleProperty));
            var property = type.Properties.FirstOrDefault(m => m.Name == nameof(SingleProperty.IntProperty));
            Assert.NotNull(property);
        }

        [Theory]
        [InlineData(nameof(MultipleProperties.ReadOnlyProperty), "System.Int32")]
        [InlineData(nameof(MultipleProperties.WriteOnlyProperty), "System.String")]
        [InlineData(nameof(MultipleProperties.ReadWriteProperty), "AsmResolver.DotNet.TestCases.Properties.MultipleProperties")]
        [InlineData("Item", "System.Int32")]
        public void ReadReturnType(string propertyName, string expectedReturnType)
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            var property = type.Properties.First(m => m.Name == propertyName);
            Assert.Equal(expectedReturnType, property.Signature.ReturnType.FullName);
        }

        [Fact]
        public void ReadDeclaringType()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var property = (PropertyDefinition) module.LookupMember(
                typeof(SingleProperty).GetProperty(nameof(SingleProperty.IntProperty)).MetadataToken);
            Assert.NotNull(property.DeclaringType);
            Assert.Equal(nameof(SingleProperty), property.DeclaringType.Name);
        }

        [Fact]
        public void ReadReadOnlyPropertySemantics()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            var property = type.Properties.First(m => m.Name == nameof(MultipleProperties.ReadOnlyProperty));
            Assert.Single(property.Semantics);
            Assert.Equal(MethodSemanticsAttributes.Getter, property.Semantics[0].Attributes);
            Assert.Same(property, property.Semantics[0].Association);
            Assert.Equal("get_ReadOnlyProperty", property.Semantics[0].Method.Name);
            Assert.NotNull(property.GetMethod);
            Assert.Null(property.SetMethod);
        }

        [Fact]
        public void ReadWriteOnlyPropertySemantics()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            var property = type.Properties.First(m => m.Name == nameof(MultipleProperties.WriteOnlyProperty));
            Assert.Single(property.Semantics);
            Assert.Equal(MethodSemanticsAttributes.Setter, property.Semantics[0].Attributes);
            Assert.Same(property, property.Semantics[0].Association);
            Assert.Equal("set_WriteOnlyProperty", property.Semantics[0].Method.Name);
            Assert.NotNull(property.SetMethod);
            Assert.Null(property.GetMethod);
        }

        [Fact]
        public void ReadReadWritePropertySemantics()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            var property = type.Properties.First(m => m.Name == nameof(MultipleProperties.ReadWriteProperty));
            Assert.Equal(2, property.Semantics.Count);
        }

        [Fact]
        public void ReadParameterlessPropertyFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var property = (PropertyDefinition) module.LookupMember(
                typeof(MultipleProperties).GetProperty(nameof(MultipleProperties.ReadOnlyProperty)).MetadataToken);

            Assert.Equal("System.Int32 AsmResolver.DotNet.TestCases.Properties.MultipleProperties::ReadOnlyProperty", property.FullName);
        }

        [Fact]
        public void ReadParameterPropertyFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var property = (PropertyDefinition) module.LookupMember(
                typeof(MultipleProperties).GetProperty("Item").MetadataToken);

            Assert.Equal("System.Int32 AsmResolver.DotNet.TestCases.Properties.MultipleProperties::Item[System.Int32]", property.FullName);
        }

        [Fact]
        public void GetMethodSetMethodProperties()
        {
            var module = new ModuleDefinition("TestModule");
            var type = new TypeDefinition("Namespace", "Name", TypeAttributes.Public);
            module.TopLevelTypes.Add(type);

            var get1 = new MethodDefinition("get_Property1", default, MethodSignature.CreateInstance(module.CorLibTypeFactory.Int32));
            var get2 = new MethodDefinition("get_Property2", default, MethodSignature.CreateInstance(module.CorLibTypeFactory.Int32));
            var set = new MethodDefinition("set_Property", default, MethodSignature.CreateInstance(module.CorLibTypeFactory.Void, module.CorLibTypeFactory.Int32));
            type.Methods.Add(get1);
            type.Methods.Add(get2);
            type.Methods.Add(set);

            Assert.False(get1.IsGetMethod || get1.IsSetMethod);
            Assert.False(get2.IsGetMethod || get2.IsSetMethod);
            Assert.False(set.IsGetMethod || set.IsSetMethod);

            var property = new PropertyDefinition("Property", PropertyAttributes.None, PropertySignature.CreateInstance(module.CorLibTypeFactory.Int32));
            type.Properties.Add(property);
            property.SetSemanticMethods(get1, set);

            Assert.True(get1.IsGetMethod);
            Assert.False(get2.IsGetMethod);
            Assert.True(set.IsSetMethod);

            property.GetMethod = get2;

            Assert.False(get1.IsGetMethod);
            Assert.True(get2.IsGetMethod);
            Assert.True(set.IsSetMethod);
        }
    }
}
