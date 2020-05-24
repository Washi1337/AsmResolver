using AsmResolver.PE.Win32Resources;
using Xunit;

namespace AsmResolver.PE.Tests.Win32Resources
{
    public class ResourceDataTest
    {
        [Fact]
        public void ResourceDataConstructorShouldSetId()
        {
            var data = new ResourceData(1, new DataSegment(new byte[0]));
            Assert.Equal(1u, data.Id);
        }
        
        [Fact]
        public void ResourceDataConstructorShouldSetContents()
        {
            var rawData = new byte[]
            {
                1, 2, 3, 4
            };
            var data = new ResourceData(1, new DataSegment(rawData));
            Assert.Equal(rawData, data.Contents.CreateReader().ReadToEnd());
        }
    }
}