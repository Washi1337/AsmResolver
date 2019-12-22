using System.Linq;
using AsmResolver.DotNet.TestCases.Properties;
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

    }
}