using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Builder.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Blob;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class BlobStreamBufferTest
    {
        [Fact]
        public void AddDistinct()
        {
            var buffer = new BlobStreamBuffer();

            var blob1 = new byte[]
            {
                1, 2, 3
            };
            uint index1 = buffer.GetBlobIndex(blob1);

            var blob2 = new byte[]
            {
                4, 5, 6
            };
            uint index2 = buffer.GetBlobIndex(blob2);

            Assert.NotEqual(index1, index2);
            
            var blobStream = buffer.CreateStream();
            Assert.Equal(blob1, blobStream.GetBlobByIndex(index1));
            Assert.Equal(blob2, blobStream.GetBlobByIndex(index2));
        }

        [Fact]
        public void AddDuplicate()
        {
            var buffer = new BlobStreamBuffer();

            var blob1 = new byte[]
            {
                1, 2, 3
            };
            uint index1 = buffer.GetBlobIndex(blob1);

            var blob2 = new byte[]
            {
                1, 2, 3
            };
            uint index2 = buffer.GetBlobIndex(blob2);

            Assert.Equal(index1, index2);

            var blobStream = buffer.CreateStream();
            Assert.Equal(blob1, blobStream.GetBlobByIndex(index1));
        }

        [Fact]
        public void AddRaw()
        {
            var buffer = new BlobStreamBuffer();

            var blob1 = new byte[]
            {
                3, 1, 2, 3
            };
            uint index1 = buffer.AppendRawData(blob1);

            var blob2 = new byte[]
            {
                1, 2, 3
            };
            uint index2 = buffer.GetBlobIndex(blob2);

            Assert.NotEqual(index1, index2);

            var blobStream = buffer.CreateStream();
            Assert.Equal(blob2, blobStream.GetBlobByIndex(index2));
        }

        [Fact]
        public void ImportBlobStreamShouldIndexExistingBlobs()
        {
            var existingBlobStream = new SerializedBlobStream(BlobStream.DefaultName, new byte[]
            {
                0,
                3, 0, 1, 2,
                5, 0, 1, 2, 3, 4,
                2, 0, 1
            });
            
            var buffer = new BlobStreamBuffer();
            buffer.ImportBlobStream(existingBlobStream);
            var newStream = buffer.CreateStream();

            Assert.Equal(new byte[]
            {
                0, 1, 2
            }, newStream.GetBlobByIndex(1));

            Assert.Equal(new byte[]
            {
                0, 1, 2, 3, 4
            }, newStream.GetBlobByIndex(5));

            Assert.Equal(new byte[]
            {
                0, 1
            }, newStream.GetBlobByIndex(11));
        }

        [Fact]
        public void ImportBlobStreamWithDuplicateBlobs()
        { 
            var existingBlobStream = new SerializedBlobStream(BlobStream.DefaultName, new byte[]
            {
                0,
                3, 0, 1, 2,
                3, 0, 1, 2,
                3, 0, 1, 2,
            });
            
            var buffer = new BlobStreamBuffer();
            buffer.ImportBlobStream(existingBlobStream);

            var newStream = buffer.CreateStream();
            
            Assert.Equal(new byte[]
            {
                0, 1, 2
            }, newStream.GetBlobByIndex(1));
            
            Assert.Equal(new byte[]
            {
                0, 1, 2
            }, newStream.GetBlobByIndex(5));
            
            Assert.Equal(new byte[]
            {
                0, 1, 2
            }, newStream.GetBlobByIndex(9));
        }

        [Fact]
        public void ImportBlobStreamWithGarbageData()
        {
            var existingBlobStream = new SerializedBlobStream(BlobStream.DefaultName, new byte[]
            {
                0,
                3, 0, 1, 2,
                123,
                3, 0, 1, 2,
            });

            var buffer = new BlobStreamBuffer();
            buffer.ImportBlobStream(existingBlobStream);

            var newStream = buffer.CreateStream();

            Assert.Equal(new byte[]
            {
                0, 1, 2
            }, newStream.GetBlobByIndex(1));

            Assert.Equal(new byte[]
            {
                0, 1, 2
            }, newStream.GetBlobByIndex(6));
        }

        [Fact]
        public void ImportBlobStreamWithUnoptimalSizedBlobHeaders()
        {
            var existingBlobStream = new SerializedBlobStream(BlobStream.DefaultName, new byte[]
            {
                0,
                3, 0, 1, 2,
                0x80,
                3, 0, 1, 2,
            });

            var buffer = new BlobStreamBuffer();
            buffer.ImportBlobStream(existingBlobStream);

            var newStream = buffer.CreateStream();

            Assert.Equal(new byte[]
            {
                0, 1, 2
            }, newStream.GetBlobByIndex(1));

            Assert.Equal(new byte[]
            {
                0, 1, 2
            }, newStream.GetBlobByIndex(5));
        }
    }
}