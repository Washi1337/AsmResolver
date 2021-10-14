using System.IO;
using System.Text;
using AsmResolver.IO;
using Xunit;

namespace AsmResolver.Tests.IO
{
    public class BinaryStreamWriterTest
    {
        [Fact]
        public void WriteByte()
        {
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            writer.WriteByte(0x80);
            writer.WriteSByte(-1);

            Assert.Equal(new byte[]
            {
                0x80,
                0xFF
            }, stream.ToArray());
        }

        [Fact]
        public void WriteInt16()
        {
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            writer.WriteUInt16(0x8001);
            writer.WriteInt16(-32766);

            Assert.Equal(new byte[]
            {
                0x01, 0x80,
                0x02, 0x80
            }, stream.ToArray());
        }

        [Fact]
        public void WriteInt32()
        {
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            writer.WriteUInt32(0x81020304u);
            writer.WriteInt32(-2063202552);

            Assert.Equal(new byte[]
            {
                0x04, 0x03, 0x02, 0x81,
                0x08, 0x07, 0x06, 0x85
            }, stream.ToArray());
        }

        [Fact]
        public void WriteInt64()
        {
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            writer.WriteUInt64(0x8001020304050607ul);
            writer.WriteInt64(-8644366967197856241);

            Assert.Equal(new byte[]
            {
                0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x80,
                0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x88,
            }, stream.ToArray());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0b1000_0000)]
        [InlineData(0b100000_0000_0000)]
        [InlineData(int.MaxValue)]
        public void Write7BitEncodedInt32(int value)
        {
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            writer.Write7BitEncodedInt32(value);

            var reader = ByteArrayDataSource.CreateReader(stream.ToArray());
            Assert.Equal(value, reader.Read7BitEncodedInt32());
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello, world!")]
        [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
        public void WriteBinaryFormatterString(string value)
        {
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            writer.WriteBinaryFormatterString(value);

            stream.Position = 0;
            var reader = new BinaryReader(stream, Encoding.UTF8);
            Assert.Equal(value, reader.ReadString());
        }

    }
}
