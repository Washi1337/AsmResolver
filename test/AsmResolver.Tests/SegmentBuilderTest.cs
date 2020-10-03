using System.IO;
using Xunit;

namespace AsmResolver.Tests
{
    public class SegmentBuilderTest
    {
        private static byte[] ToBytes(ISegment segment)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                segment.Write(writer);
                return stream.ToArray();
            }
        }
        
        [Fact]
        public void EmptyNoAlignment()
        {
            var collection = new SegmentBuilder();
            
            collection.UpdateOffsets(0x400, 0x1000);
            
            Assert.Equal(0x400u, collection.Offset);
            Assert.Equal(0x1000u, collection.Rva);
            Assert.Equal(0u, collection.GetPhysicalSize());
            Assert.Equal(0u, collection.GetVirtualSize());
            
            Assert.Empty(ToBytes(collection));
        }

        [Fact]
        public void SingleItemNoAlignment()
        {
            var segment = new DataSegment(new byte[] {1, 2, 3, 4});

            var collection = new SegmentBuilder {segment};
            
            collection.UpdateOffsets(0x400, 0x1000);
            
            Assert.Equal(0x400u, segment.Offset);
            Assert.Equal(0x1000u, segment.Rva);
            Assert.Equal(4u, collection.GetPhysicalSize());
            Assert.Equal(4u, collection.GetVirtualSize());

            Assert.Equal(new byte[]
            {
                1, 2, 3, 4
            }, ToBytes(collection));
        }

        [Fact]
        public void MultipleItemsNoAlignment()
        {
            var segment1 = new DataSegment(new byte[] {1, 2, 3, 4});
            var segment2 = new DataSegment(new byte[] {1, 2, 3 });
            var segment3 = new DataSegment(new byte[] {1, 2, 3, 4, 5});

            var collection = new SegmentBuilder {segment1, segment2, segment3};
            
            collection.UpdateOffsets(0x400, 0x1000);
            
            Assert.Equal(0x400u, segment1.Offset);
            Assert.Equal(0x1000u, segment1.Rva);
            Assert.Equal(0x404u, segment2.Offset);
            Assert.Equal(0x1004u, segment2.Rva);
            Assert.Equal(0x407u, segment3.Offset);
            Assert.Equal(0x1007u, segment3.Rva);
            
            Assert.Equal(12u, collection.GetPhysicalSize());
            Assert.Equal(12u, collection.GetVirtualSize());

            Assert.Equal(new byte[]
            {
                1, 2, 3, 4,
                1, 2, 3,
                1, 2, 3, 4, 5
            }, ToBytes(collection));
        }

        [Fact]
        public void SingleItemAlignment()
        {
            var segment = new DataSegment(new byte[] {1, 2, 3, 4});

            var builder = new SegmentBuilder {segment};
            
            builder.UpdateOffsets(0x400, 0x1000);

            Assert.Equal(0x400u, segment.Offset);
            
            Assert.Equal(4u, builder.GetPhysicalSize());
            Assert.Equal(4u, builder.GetVirtualSize());

            Assert.Equal(new byte[]
            {
                1, 2, 3, 4
            }, ToBytes(builder));
        }

        [Fact]
        public void MultipleItemsAlignment()
        {
            var segment1 = new DataSegment(new byte[] {1, 2, 3, 4});
            var segment2 = new DataSegment(new byte[] {1, 2, 3 });
            var segment3 = new DataSegment(new byte[] {1, 2, 3, 4, 5});

            var builder = new SegmentBuilder
            {
                {segment1, 8},
                {segment2, 8}, 
                {segment3, 8}
            };

            builder.UpdateOffsets(0x400, 0x1000);
            
            Assert.Equal(0x400u, segment1.Offset);
            Assert.Equal(0x1000u, segment1.Rva);
            Assert.Equal(0x408u, segment2.Offset);
            Assert.Equal(0x1008u, segment2.Rva);
            Assert.Equal(0x410u, segment3.Offset);
            Assert.Equal(0x1010u, segment3.Rva);
            
            Assert.Equal(0x15u, builder.GetPhysicalSize());
            Assert.Equal(0x15u, builder.GetVirtualSize());

            Assert.Equal(new byte[]
            {
                1, 2, 3, 4, 0, 0, 0, 0,
                1, 2, 3, 0, 0, 0, 0, 0,
                1, 2, 3, 4, 5
            }, ToBytes(builder));
        }
        
    }
}