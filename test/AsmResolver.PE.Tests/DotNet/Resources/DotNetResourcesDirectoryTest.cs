using System.Linq;
using System.Text;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;
using TestCaseResources = AsmResolver.DotNet.TestCases.Resources.Resources;

namespace AsmResolver.PE.Tests.DotNet.Resources
{
    public class DotNetResourcesDirectoryTest
    {
        private static ManifestResourceRow FindResourceRow(MetadataDirectory metadata, string name)
        {
            var stringsStream = metadata.GetStream<StringsStream>();
            var tablesStream = metadata.GetStream<TablesStream>();

            var table = tablesStream.GetTable<ManifestResourceRow>(TableIndex.ManifestResource);
            var resource = table.First(t => stringsStream.GetStringByIndex(t.Name) == name);
            return resource;
        }

        [Fact]
        public void ReadEmbeddedResource1Data()
        {
            var image = PEImage.FromFile(typeof(TestCaseResources).Assembly.Location);
            var metadata = image.DotNetDirectory!.Metadata!;
            var resource = FindResourceRow(metadata, "AsmResolver.DotNet.TestCases.Resources.Resources.EmbeddedResource1");

            byte[]? data = image.DotNetDirectory.DotNetResources!.GetManifestResourceData(resource.Offset);
            Assert.NotNull(data);
            Assert.Equal(TestCaseResources.GetEmbeddedResource1Data(), Encoding.ASCII.GetString(data!));
        }

        [Fact]
        public void ReadEmbeddedResource2Data()
        {
            var image = PEImage.FromFile(typeof(TestCaseResources).Assembly.Location);
            var metadata = image.DotNetDirectory!.Metadata!;
            var resource = FindResourceRow(metadata, "AsmResolver.DotNet.TestCases.Resources.Resources.EmbeddedResource2");

            byte[]? data = image.DotNetDirectory.DotNetResources!.GetManifestResourceData(resource.Offset);
            Assert.NotNull(data);
            Assert.Equal(TestCaseResources.GetEmbeddedResource2Data(), Encoding.ASCII.GetString(data!));
        }
    }
}
