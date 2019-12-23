using System.Linq;
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
        }

        [Fact]
        public void ReadReadWritePropertySemantics()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            var property = type.Properties.First(m => m.Name == nameof(MultipleProperties.ReadWriteProperty));
            Assert.Equal(2, property.Semantics.Count);
        }
    }
}