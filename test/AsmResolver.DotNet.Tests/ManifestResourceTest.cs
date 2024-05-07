using System.IO;
using System.Linq;
using System.Text;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.Tests.Runners;
using Xunit;
using TestCaseResources = AsmResolver.DotNet.TestCases.Resources.Resources;

namespace AsmResolver.DotNet.Tests
{
    public class ManifestResourceTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public ManifestResourceTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

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
            var resource = newModule.Resources.First(r => r.Name == resourceName);

            Assert.True(resource.TryGetReader(out var reader));
            Assert.Equal(contents, reader.ReadToEnd());
        }

        [Fact]
        public void PersistentUniqueResourceData()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.DupResource);

            // Create three unique resources.
            module.Resources.Add(new ManifestResource(
                "resource1",
                ManifestResourceAttributes.Public,
                new DataSegment(new byte[] {1, 2, 3, 4}))
            );
            module.Resources.Add(new ManifestResource(
                "resource2",
                ManifestResourceAttributes.Public,
                new DataSegment(new byte[] {5, 6, 7, 8}))
            );
            module.Resources.Add(new ManifestResource(
                "resource3",
                ManifestResourceAttributes.Public,
                new DataSegment(new byte[] {9, 10, 11, 12}))
            );

            // Verify program returns correct data.
            _fixture
                .GetRunner<CorePERunner>()
                .RebuildAndRun(module, "DupResource.dll",
                    """
                    resource1: 01020304
                    resource2: 05060708
                    resource3: 090A0B0C

                    """);
        }

        [Theory]
        [InlineData(MetadataBuilderFlags.None)]
        [InlineData(MetadataBuilderFlags.NoResourceDataDeduplication)]
        public void PersistentIdenticalResourceData(MetadataBuilderFlags flags)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.DupResource);

            // Create two identical resources and one unique resource.
            module.Resources.Add(new ManifestResource(
                "resource1",
                ManifestResourceAttributes.Public,
                new DataSegment(new byte[] {1, 2, 3, 4}))
            );
            module.Resources.Add(new ManifestResource(
                "resource2",
                ManifestResourceAttributes.Public,
                new DataSegment(new byte[] {1, 2, 3, 4}))
            );
            module.Resources.Add(new ManifestResource(
                "resource3",
                ManifestResourceAttributes.Public,
                new DataSegment(new byte[] {9, 10, 11, 12}))
            );

            // Build image.
            var image = module.ToPEImage(new ManagedPEImageBuilder(flags));

            var table = image.DotNetDirectory!.Metadata!
                .GetStream<TablesStream>()
                .GetTable<ManifestResourceRow>();

            if ((flags & MetadataBuilderFlags.NoResourceDataDeduplication) != 0)
            {
                // Without deduplication, all offsets should be different.
                Assert.NotEqual(table[0].Offset, table[1].Offset);
                Assert.NotEqual(table[0].Offset, table[2].Offset);
            }
            else
            {
                // With deduplication, resources with same data should share data offset.
                Assert.Equal(table[0].Offset, table[1].Offset);
                Assert.NotEqual(table[0].Offset, table[2].Offset);
            }

            // Verify program returns correct data.
            var file = new ManagedPEFileBuilder().CreateFile(image);
            _fixture
                .GetRunner<CorePERunner>()
                .RebuildAndRun(file, $"DupResource_{flags}.dll",
                    """
                    resource1: 01020304
                    resource2: 01020304
                    resource3: 090A0B0C

                    """);
        }
    }
}
