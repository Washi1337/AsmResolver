using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.UserStrings;
using AsmResolver.PE.File;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata
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

        [Fact]
        public void DetectUserStringsStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory.Metadata;

            var stream = metadata.GetStream(UserStringsStream.DefaultName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<UserStringsStream>(stream);
        }

        [Fact]
        public void DetectBlobStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory.Metadata;

            var stream = metadata.GetStream(BlobStream.DefaultName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<BlobStream>(stream);
        }

        [Fact]
        public void DetectGuidStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory.Metadata;

            var stream = metadata.GetStream(GuidStream.DefaultName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<GuidStream>(stream);
        }

        [Fact]
        public void DetectCompressedTableStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory.Metadata;

            var stream = metadata.GetStream(TablesStream.CompressedStreamName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<TablesStream>(stream);
        }

        [Fact]
        public void PreserveMetadataNoChange()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            var peImage = PEImage.FromFile(peFile);
            var metadata = peImage.DotNetDirectory.Metadata;

            using var tempStream = new MemoryStream();
            metadata.Write(new BinaryStreamWriter(tempStream));

            var reader = ByteArrayReaderFactory.CreateReader(tempStream.ToArray());
            var newMetadata = new SerializedMetadata(new PEReaderContext(peFile), ref reader);

            Assert.Equal(metadata.MajorVersion, newMetadata.MajorVersion);
            Assert.Equal(metadata.MinorVersion, newMetadata.MinorVersion);
            Assert.Equal(metadata.Reserved, newMetadata.Reserved);
            Assert.Equal(metadata.VersionString, newMetadata.VersionString);
            Assert.Equal(metadata.Flags, newMetadata.Flags);

            Assert.Equal(metadata.Streams.Count, newMetadata.Streams.Count);
            for (int i = 0; i < metadata.Streams.Count; i++)
            {
                var oldStream = metadata.Streams[i];
                var newStream = newMetadata.Streams[i];

                Assert.Equal(oldStream.Name, newStream.Name);
                var oldData = oldStream.CreateReader().ReadToEnd();
                var newData = newStream.CreateReader().ReadToEnd();
                Assert.Equal(oldData, newData);

            }
        }


    }
}
