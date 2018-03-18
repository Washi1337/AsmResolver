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