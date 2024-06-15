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
        public void FindNullBlob() => AssertHasBlob(new byte[]
            {
                0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            },
            null);

        [Fact]
        public void FindSmallExistingBlob() => AssertHasBlob(new byte[]
            {
                0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            },
            new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05
            });

        [Fact]
        public void FindSmallExistingBlobAfterSimilarBlob() => AssertHasBlob(new byte[]
            {
                0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x06, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            },
            new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05
            });

        [Fact]
        public void FindSmallOverlappingBlob() => AssertHasBlob(new byte[]
            {
                0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            },
            new byte[]
            {
                0x02
            });

        [Fact]
        public void FindSmallOverlappingBlob2() => AssertHasBlob(new byte[]
            {
                0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            },
            new byte[]
            {
                0x03, 0x04
            });

        [Fact]
        public void FindSmallIncompleteBlobShouldReturnFalse() => AssertDoesNotHaveBlob(new byte[]
            {
                0x00, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            },
            new byte[]
            {
                0x04, 0x05, 0x06, 0x07
            });

        [Fact]
        public void FindSmallBlobAfterCorruptedHeader() => AssertHasBlob(new byte[]
            {
                0x00, 0x80, 0x03, 0x03, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            },
            new byte[]
            {
                0x01, 0x02, 0x03
            });
    }
}
