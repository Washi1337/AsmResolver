using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
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

        [Fact]
        public void PersistentData()
        {
            const string resourceName = "SomeResource";
            var contents = new byte[]
            {
                0,1,2,3,4
            };

            var module = ModuleDefinition.FromFile(typeof(TestCaseResources).Assembly.Location);
            module.Resources.Add(new ManifestResource(resourceName, ManifestResourceAttributes.Public, new DataSegment(contents)));

            using var stream = new MemoryStream();
            module.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            Assert.Equal(contents, newModule.Resources.First(r => r.Name == resourceName).GetData());
        }

        [Fact]
        public void PersistentDataReader()
        {
            const string resourceName = "SomeResource";
            var contents = new byte[]
            {
                0,1,2,3,4
            };

            var module = ModuleDefinition.FromFile(typeof(TestCaseResources).Assembly.Location);
            module.Resources.Add(new ManifestResource(resourceName, ManifestResourceAttributes.Public, new DataSegment(contents)));

            using var stream = new MemoryStream();
            module.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            Assert.Equal(contents
                , newModule.Resources.First(r => r.Name == resourceName).GetReader()?.ReadToEnd());
        }
    }
}
