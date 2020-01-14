using System;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Guid;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class GuidStreamBufferTest
    {
        [Fact]
        public void AddDistinct()
        {
            var buffer = new GuidStreamBuffer();

            var guid1 = new Guid(new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            });
            uint index1 = buffer.GetGuidIndex(guid1);

            var guid2 = new Guid(new byte[]
            {
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F
            });
            uint index2 = buffer.GetGuidIndex(guid2);

            Assert.NotEqual(index1, index2);
            
            var blobStream = (GuidStream) buffer.CreateStream();
            Assert.Equal(guid1, blobStream.GetGuidByIndex(index1));
            Assert.Equal(guid2, blobStream.GetGuidByIndex(index2));
        }

        [Fact]
        public void AddDuplicate()
        {
            var buffer = new GuidStreamBuffer();

            var guid1 = new Guid(new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            });
            uint index1 = buffer.GetGuidIndex(guid1);

            var guid2 =  new Guid(new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            });
            uint index2 = buffer.GetGuidIndex(guid2);

            Assert.Equal(index1, index2);

            var blobStream = (GuidStream) buffer.CreateStream();
            Assert.Equal(guid1, blobStream.GetGuidByIndex(index1));
        }

    }
}