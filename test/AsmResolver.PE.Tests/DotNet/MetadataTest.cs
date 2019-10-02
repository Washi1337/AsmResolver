using System.Linq;
using AsmResolver.PE.DotNet.Metadata;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet
{
    public class MetadataTest
    {
        [Fact]
        public void CorrectHeader()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory.Metadata;
            
            Assert.Equal(1, metadata.MajorVersion);
            Assert.Equal(1, metadata.MinorVersion);
            Assert.Equal(0u, metadata.Reserved);
            Assert.Contains("v4.0.30319", metadata.VersionString);
            Assert.Equal(0u, metadata.Flags);
            Assert.Equal(5, metadata.Streams.Count);
        }

        [Fact]
        public void CorrectStreamHeaders()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory.Metadata;

            var expectedNames = new[] {"#~", "#Strings", "#US", "#GUID", "#Blob"};
            Assert.Equal(expectedNames, metadata.Streams.Select(s => s.Name));
        }

        [Fact]
        public void DetectStringsStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory.Metadata;

            var stream = metadata.GetStream(StringsStream.DefaultName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<StringsStream>(stream);
        }
        
    }
}