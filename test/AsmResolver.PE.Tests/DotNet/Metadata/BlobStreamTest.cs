using AsmResolver.PE.DotNet.Metadata;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata
{
    public class BlobStreamTest
    {
        private static void AssertDoesNotHaveBlob(byte[] streamData, byte[] needle)
        {
            var stream = new SerializedBlobStream(streamData);
            Assert.False(stream.TryFindBlobIndex(needle, out _));
        }

        private static void AssertHasBlob(byte[] streamData, byte[]? needle)
        {
            var stream = new SerializedBlobStream(streamData);
            Assert.True(stream.TryFindBlobIndex(needle, out uint actualIndex));
            Assert.Equal(needle, stream.GetBlobByIndex(actualIndex));
        }

        [Fact]
        public void FindNullBlob() => AssertHasBlob([
                0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            ],
            null);

        [Fact]
        public void FindSmallExistingBlob()
        {
            AssertHasBlob(
                [0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06],
                [0x01, 0x02, 0x03, 0x04, 0x05]
            );
        }

        [Fact]
        public void FindSmallExistingBlobAfterSimilarBlob()
        {
            AssertHasBlob(
                [0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x06, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06],
                [0x01, 0x02, 0x03, 0x04, 0x05]
            );
        }

        [Fact]
        public void FindSmallOverlappingBlob()
        {
            AssertHasBlob(
                [0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06],
                [0x02]
            );
        }

        [Fact]
        public void FindSmallOverlappingBlob2()
        {
            AssertHasBlob(
                [0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06],
                [0x03, 0x04]
            );
        }

        [Fact]
        public void FindSmallIncompleteBlobShouldReturnFalse()
        {
            AssertDoesNotHaveBlob(
                [0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06],
                [0x04, 0x05, 0x06, 0x07]
            );
        }

        [Fact]
        public void FindSmallBlobAfterCorruptedHeader()
        {
            AssertHasBlob(
                [0x00, 0x80, 0x03, 0x03, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06],
                [0x01, 0x02, 0x03]
            );
        }

        [Fact]
        public void EnumerateBlobs()
        {
            var stream = new SerializedBlobStream([0x00, 0x01, 0x01, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03]);
            Assert.Equal([
                (1, [0x01]),
                (3, [0x02, 0x02]),
                (6, [0x03, 0x03, 0x03])
            ], stream.EnumerateBlobs());
        }

        [Fact]
        public void EnumerateBlobsMalicious()
        {
            var stream = new SerializedBlobStream([0x00, 0x20, 0x01, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03]);
            Assert.Equal([
                (1, [0x01, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03])
            ], stream.EnumerateBlobs());
        }

        [Fact]
        public void EnumerateEmptyBlobs()
        {
            var stream = new SerializedBlobStream([0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x03, 0x03, 0x03, 0x03]);
            Assert.Equal([
                (1, [0x01]),
                (3, []),
                (4, []),
                (5, []),
                (6, [0x03, 0x03, 0x03])
            ], stream.EnumerateBlobs());
        }
    }
}
