using System.Linq;
using System.Text;
using Xunit;
using TestCaseResources = AsmResolver.DotNet.TestCases.Resources.Resources;

namespace AsmResolver.DotNet.Tests
{
    public class ManifestResourceTest
    {
        [Fact]
        public void ReadEmbeddedResource1Data()
        {
            var module = ModuleDefinition.FromFile(typeof(TestCaseResources).Assembly.Location);
            var resource = module.Resources.First(r =>
                r.Name == "AsmResolver.DotNet.TestCases.Resources.Resources.EmbeddedResource1");

            Assert.Equal(TestCaseResources.GetEmbeddedResource1Data(), Encoding.ASCII.GetString(resource.GetData()));
        }

        [Fact]
        public void ReadEmbeddedResource2Data()
        {
            var module = ModuleDefinition.FromFile(typeof(TestCaseResources).Assembly.Location);
            var resource = module.Resources.First(r =>
                r.Name == "AsmResolver.DotNet.TestCases.Resources.Resources.EmbeddedResource2");

            Assert.Equal(TestCaseResources.GetEmbeddedResource2Data(), Encoding.ASCII.GetString(resource.GetData()));
        }
    }
}