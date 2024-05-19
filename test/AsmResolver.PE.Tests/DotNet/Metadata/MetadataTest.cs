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
            var metadata = peImage.DotNetDirectory!.Metadata!;

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
            var metadata = peImage.DotNetDirectory!.Metadata!;

            string[] expectedNames = new[] {"#~", "#Strings", "#US", "#GUID", "#Blob"};
            Assert.Equal(expectedNames, metadata.Streams.Select(s => s.Name));
        }

        [Fact]
        public void CorrectStreamHeadersUnalignedMetadataDirectory()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_UnalignedMetadata);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            string[] expectedNames = new[] {"#~", "#Strings", "#US", "#GUID", "#Blob"};
            Assert.Equal(expectedNames, metadata.Streams.Select(s => s.Name));
        }

        [Fact]
        public void DetectStringsStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            var stream = metadata.GetStream(StringsStream.DefaultName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<StringsStream>(stream);
        }

        [Fact]
        public void DetectUserStringsStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            var stream = metadata.GetStream(UserStringsStream.DefaultName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<UserStringsStream>(stream);
        }

        [Fact]
        public void DetectBlobStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            var stream = metadata.GetStream(BlobStream.DefaultName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<BlobStream>(stream);
        }

        [Fact]
        public void DetectGuidStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            var stream = metadata.GetStream(GuidStream.DefaultName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<GuidStream>(stream);
        }

        [Fact]
        public void DetectCompressedTableStream()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            var stream = metadata.GetStream(TablesStream.CompressedStreamName);
            Assert.NotNull(stream);
            Assert.IsAssignableFrom<TablesStream>(stream);
        }

        [Fact]
        public void PreserveMetadataNoChange()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            var peImage = PEImage.FromFile(peFile);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            using var tempStream = new MemoryStream();
            metadata.Write(new BinaryStreamWriter(tempStream));

            var reader = new BinaryStreamReader(tempStream.ToArray());
            var context = MetadataReaderContext.FromReaderContext(new PEReaderContext(peFile));
            var newMetadata = new SerializedMetadata(context, ref reader);

            Assert.Equal(metadata.MajorVersion, newMetadata.MajorVersion);
            Assert.Equal(metadata.MinorVersion, newMetadata.MinorVersion);
            Assert.Equal(metadata.Reserved, newMetadata.Reserved);
            Assert.Equal(metadata.VersionString, newMetadata.VersionString);
            Assert.Equal(metadata.Flags, newMetadata.Flags);

            Assert.Equal(metadata.Streams.Count, newMetadata.Streams.Count);
            Assert.All(Enumerable.Range(0, metadata.Streams.Count), i =>
            {
                var oldStream = metadata.Streams[i];
                var newStream = newMetadata.Streams[i];

                Assert.Equal(oldStream.Name, newStream.Name);
                var oldData = oldStream.CreateReader().ReadToEnd();
                var newData = newStream.CreateReader().ReadToEnd();
                Assert.Equal(oldData, newData);
            });
        }

        private void AssertCorrectStreamIsSelected<TStream>(byte[] assembly, bool isEnC)
            where TStream : class, IMetadataStream
        {
            AssertCorrectStreamIsSelected<TStream>(PEImage.FromBytes(assembly), isEnC);
        }

        private void AssertCorrectStreamIsSelected<TStream>(IPEImage peImage, bool isEnC)
            where TStream : class, IMetadataStream
        {
            var metadata = peImage.DotNetDirectory!.Metadata!;

            var allStreams = metadata.Streams
                .OfType<TStream>()
                .ToArray();

            var dominantStream = metadata.GetStream<TStream>();
            int expectedIndex = isEnC ? 0 : allStreams.Length - 1;
            Assert.Equal(allStreams[expectedIndex], dominantStream);
        }

        [Fact]
        public void SelectLastBlobStreamInNormalMetadata()
        {
            AssertCorrectStreamIsSelected<BlobStream>(Properties.Resources.HelloWorld_DoubleBlobStream, false);
        }

        [Fact]
        public void SelectLastGuidStreamInNormalMetadata()
        {
            AssertCorrectStreamIsSelected<GuidStream>(Properties.Resources.HelloWorld_DoubleGuidStream, false);
        }

        [Fact]
        public void SelectLastStringsStreamInNormalMetadata()
        {
            AssertCorrectStreamIsSelected<StringsStream>(Properties.Resources.HelloWorld_DoubleStringsStream, false);
        }

        [Fact]
        public void SelectLastUserStringsStreamInNormalMetadata()
        {
            AssertCorrectStreamIsSelected<UserStringsStream>(Properties.Resources.HelloWorld_DoubleUserStringsStream, false);
        }

        [Fact]
        public void SelectFirstBlobStreamInEnCMetadata()
        {
            AssertCorrectStreamIsSelected<BlobStream>(Properties.Resources.HelloWorld_DoubleBlobStream_EnC, true);
        }

        [Fact]
        public void SelectFirstGuidStreamInEnCMetadata()
        {
            AssertCorrectStreamIsSelected<GuidStream>(Properties.Resources.HelloWorld_DoubleGuidStream_EnC, true);
        }

        [Fact]
        public void SelectFirstStringsStreamInEnCMetadata()
        {
            AssertCorrectStreamIsSelected<StringsStream>(Properties.Resources.HelloWorld_DoubleStringsStream_EnC, true);
        }

        [Fact]
        public void SelectFirstUserStringsStreamInEnCMetadata()
        {
            AssertCorrectStreamIsSelected<UserStringsStream>(Properties.Resources.HelloWorld_DoubleUserStringsStream_EnC, true);
        }

        [Fact]
        public void SchemaStreamShouldForceEnCMetadata()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_SchemaStream);
            AssertCorrectStreamIsSelected<BlobStream>(peImage, true);
            AssertCorrectStreamIsSelected<GuidStream>(peImage, true);
            AssertCorrectStreamIsSelected<StringsStream>(peImage, true);
            AssertCorrectStreamIsSelected<UserStringsStream>(peImage, true);
        }

        [Fact]
        public void UseCaseInsensitiveComparisonForHeapNamesInEnCMetadata()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_LowerCaseHeapsWithEnC);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            Assert.True(metadata.TryGetStream(out BlobStream _));
            Assert.True(metadata.TryGetStream(out GuidStream _));
            Assert.True(metadata.TryGetStream(out StringsStream _));
            Assert.True(metadata.TryGetStream(out UserStringsStream _));
        }

        [Fact]
        public void UseLargeTableIndicesWhenJTDStreamIsPresentInEnCMetadata()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_JTDStream);
            var metadata = peImage.DotNetDirectory!.Metadata!;

            var tablesStream = metadata.GetStream<TablesStream>();

            Assert.True(tablesStream.ForceLargeColumns);

            for (var i = TableIndex.Module; i <= TableIndex.GenericParamConstraint; i++)
                Assert.Equal(IndexSize.Long, tablesStream.GetTableIndexSize(i));

            for (var i = TableIndex.Document; i <= TableIndex.CustomDebugInformation; i++)
                Assert.Equal(IndexSize.Long, tablesStream.GetTableIndexSize(i));

            for (var i = CodedIndex.TypeDefOrRef; i <= CodedIndex.HasCustomDebugInformation; i++)
                Assert.Equal(IndexSize.Long, tablesStream.GetIndexEncoder(i).IndexSize);

            Assert.Equal(IndexSize.Long, tablesStream.StringIndexSize);
            Assert.Equal(IndexSize.Long, tablesStream.GuidIndexSize);
            Assert.Equal(IndexSize.Long, tablesStream.BlobIndexSize);
        }
    }
}
