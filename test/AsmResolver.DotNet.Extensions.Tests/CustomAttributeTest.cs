using System.Linq;
using AsmResolver.DotNet.TestCases.Properties;
using Xunit;

namespace AsmResolver.DotNet.Extensions.Tests
{
    public class CustomAttributeTest
    {
        [Fact]
        public void IsCompilerGeneratedMember()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleProperty));
            var property = type.Properties.First();
            var setMethod = property.SetMethod;

            Assert.True(setMethod.IsCompilerGenerated());
        }
    }
}