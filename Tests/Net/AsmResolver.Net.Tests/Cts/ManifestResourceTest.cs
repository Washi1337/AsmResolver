using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using Xunit;

namespace AsmResolver.Tests.Net.Cts
{
    public class ManifestResourceTest
    {
        private const string DummyAssemblyName = "SomeAssembly";
        
        private static ManifestResource CreateDummyResource()
        {
            return new ManifestResource("SomeResource", ManifestResourceAttributes.Public, new byte[0]);
        }

        [Fact]
        public void PersistentData()
        {
            var newData = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            
            var resource = CreateDummyResource();
            resource.Data = newData;
            image.Assembly.Resources.Add(resource);

            var mapping = header.UnlockMetadata();

            var resourceRow = header.GetStream<TableStream>()
                .GetTable<ManifestResourceTable>()[(int) (mapping[resource].Rid - 1)];
            Assert.Equal(newData, header.NetDirectory.ResourcesManifest.GetResourceData(resourceRow.Column1));

            image = header.LockMetadata();
            resource = (ManifestResource) image.ResolveMember(mapping[resource]);
            Assert.Equal(newData, resource.Data);
        }

        [Fact]
        public void PersistentDataMultipleResources()
        {
            var newData1 = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            var newData2 = new byte[] {11, 12, 13, 14, 15, 16, 17, 18, 19, 20};
            
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            
            var resource1 = CreateDummyResource();
            resource1.Data = newData1;
            var resource2 = CreateDummyResource();
            resource2.Data = newData2;
            
            image.Assembly.Resources.Add(resource1);
            image.Assembly.Resources.Add(resource2);

            var mapping = header.UnlockMetadata();

            var manifestResourceTable = header.GetStream<TableStream>().GetTable<ManifestResourceTable>();

            var resourceRow1 = manifestResourceTable[(int) (mapping[resource1].Rid - 1)];
            Assert.Equal(newData1, header.NetDirectory.ResourcesManifest.GetResourceData(resourceRow1.Column1));
            var resourceRow2 = manifestResourceTable[(int) (mapping[resource2].Rid - 1)];
            Assert.Equal(newData2, header.NetDirectory.ResourcesManifest.GetResourceData(resourceRow2.Column1));

            image = header.LockMetadata();
            resource1 = (ManifestResource) image.ResolveMember(mapping[resource1]);
            resource2 = (ManifestResource) image.ResolveMember(mapping[resource2]);
            Assert.Equal(newData1, resource1.Data);
            Assert.Equal(newData2, resource2.Data);
        }

        [Fact]
        public void PersistentAttributes()
        {
            const ManifestResourceAttributes newAttributes = ManifestResourceAttributes.Private;
             
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            
            var resource = CreateDummyResource();
            resource.Attributes = newAttributes;
            image.Assembly.Resources.Add(resource);

            var mapping = header.UnlockMetadata();

            var resourceRow = header.GetStream<TableStream>()
                .GetTable<ManifestResourceTable>()[(int) (mapping[resource].Rid - 1)];
            Assert.Equal(newAttributes, resourceRow.Column2);

            image = header.LockMetadata();
            resource = (ManifestResource) image.ResolveMember(mapping[resource]);
            Assert.Equal(newAttributes, resource.Attributes);
        }

        [Fact]
        public void PersistentName()
        {
            const string newName = "MyResource";
            
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;

            var image = header.LockMetadata();
            
            var resource = CreateDummyResource();
            resource.Name = newName;
            image.Assembly.Resources.Add(resource);

            var mapping = header.UnlockMetadata();

            var resourceRow = header.GetStream<TableStream>()
                .GetTable<ManifestResourceTable>()[(int) (mapping[resource].Rid - 1)];
            Assert.Equal(newName, header.GetStream<StringStream>().GetStringByOffset(resourceRow.Column3));

            image = header.LockMetadata();
            resource = (ManifestResource) image.ResolveMember(mapping[resource]);
            Assert.Equal(newName, resource.Name);
        }
    }
}