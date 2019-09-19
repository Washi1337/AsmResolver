using System.IO;
using Xunit;

namespace AsmResolver.Tests
{
    public class BinaryStreamWriterTest
    {
        [Fact]
        public void WriteByte()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                
                writer.WriteByte(0x80);
                writer.WriteSByte(-1);

                Assert.Equal(new byte[]
                {
                    0x80,
                    0xFF
                }, stream.ToArray());
            }
        }
        
        [Fact]
        public void WriteInt16()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                
                writer.WriteUInt16(0x8001);
                writer.WriteInt16(-32766);

                Assert.Equal(new byte[]
                {
                    0x01, 0x80,
                    0x02, 0x80
                }, stream.ToArray());
            }
        }

        [Fact]
        public void WriteInt32()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                
                writer.WriteUInt32(0x81020304u);
                writer.WriteInt32(-2063202552);

                Assert.Equal(new byte[]
                {
                    0x04, 0x03, 0x02, 0x81,
                    0x08, 0x07, 0x06, 0x85
                }, stream.ToArray());
            }
        }

        [Fact]
        public void WriteInt64()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                
                writer.WriteUInt64(0x8001020304050607ul);
                writer.WriteInt64(-8644366967197856241);

                Assert.Equal(new byte[]
                {
                    0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x80,
                    0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x88,
                }, stream.ToArray());
            }
        }

    }
}