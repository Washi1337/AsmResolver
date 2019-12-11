using System.Linq;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.DotNet.TestCases.Methods;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MethodDefinitionTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleMethod));
            var method = type.Methods.FirstOrDefault(m => m.Name == nameof(SingleMethod.VoidParameterlessMethod));
            Assert.NotNull(method);
        }
    }
}