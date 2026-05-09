using System;
using System.IO;
using System.Text;
using AsmResolver.IO;
using Xunit;

namespace AsmResolver.Tests.IO
{
    public class BinaryStreamReaderTest
    {
        [Fact]
        public void EmptyArray()
        {
            var readerState = new BinaryStreamReaderState([]);
            Assert.Equal(0u, readerState.Length);

            Assert.Throws<EndOfStreamException>(() => readerState.CreateReader().ReadByte());
            Assert.Equal(0, readerState.CreateReader().ReadBytes(new byte[10], 0, 10));
        }

        [Fact]
        public void ReadByte()
        {
            var state = new BinaryStreamReaderState([0x80, 0x80]);
            var reader = state.CreateReader();

            Assert.Equal((byte) 0x80, reader.ReadByte());
            Assert.Equal(1u, reader.Offset);
            Assert.Equal((sbyte) -128, reader.ReadSByte());

            state = reader.GetState();
            Assert.Throws<EndOfStreamException>(() => state.CreateReader().ReadByte());
        }

        [Fact]
        public void ReadInt16()
        {
            var state = new BinaryStreamReaderState([
                0x01, 0x80,
                0x02, 0x80
            ]);

            var reader = state.CreateReader();
            Assert.Equal((ushort) 0x8001, reader.ReadUInt16());
            Assert.Equal(2u, reader.Offset);
            Assert.Equal((short) -32766, reader.ReadInt16());
            Assert.Equal(4u, reader.Offset);
        }

        [Fact]
        public void ReadInt32()
        {
            var state = new BinaryStreamReaderState([
                0x04, 0x03, 0x02, 0x81,
                0x08, 0x07, 0x06, 0x85
            ]);

            var reader = state.CreateReader();
            Assert.Equal(0x81020304u, reader.ReadUInt32());
            Assert.Equal(4u, reader.Offset);
            Assert.Equal(-2063202552, reader.ReadInt32());
            Assert.Equal(8u, reader.Offset);
        }


        [Fact]
        public void ReadInt64()
        {
            var state = new BinaryStreamReaderState([
                0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x80,
                0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x88
            ]);

            var reader = state.CreateReader();
            Assert.Equal(0x8001020304050607ul, reader.ReadUInt64());
            Assert.Equal(8u, reader.Offset);
            Assert.Equal(-8644366967197856241, reader.ReadInt64());
            Assert.Equal(16u, reader.Offset);
        }

        [Theory]
        [InlineData(new byte[] {0x03}, 3)]
        [InlineData(new byte[] {0x7f}, 0x7f)]
        [InlineData(new byte[] {0x80, 0x80}, 0x80)]
        [InlineData(new byte[] {0xAE, 0x57}, 0x2E57)]
        [InlineData(new byte[] {0xBF, 0xFF}, 0x3FFF)]
        [InlineData(new byte[] {0xC0, 0x00, 0x40, 0x00}, 0x4000)]
        [InlineData(new byte[] {0xDF, 0x12, 0x34, 0x56}, 0x1F123456)]
        [InlineData(new byte[] {0xDF, 0xFF, 0xFF, 0xFF}, 0x1FFFFFFF)]
        public void ReadCompressedUInt32(byte[] data, uint expected)
        {
            var state = new BinaryStreamReaderState(data);
            Assert.Equal(expected, state.CreateReader().ReadCompressedUInt32());
            Assert.True(state.CreateReader().TryReadCompressedUInt32(out uint value));
            Assert.Equal(expected, value);
        }

        [Theory]
        [InlineData(new byte[] {0x06}, 3)]
        [InlineData(new byte[] {0x7B}, -3)]
        [InlineData(new byte[] {0x80, 0x80}, 64)]
        [InlineData(new byte[] {0x01}, -64)]
        [InlineData(new byte[] {0xC0, 0x00, 0x40, 0x00}, 8192)]
        [InlineData(new byte[] {0x80, 0x01}, -8192)]
        [InlineData(new byte[] {0xDF, 0xFF, 0xFF, 0xFE}, 0xFFFFFFF)]
        [InlineData(new byte[] {0xC0, 0x00, 0x00, 0x01}, -0x10000000)]
        public void ReadCompressedInt32(byte[] data, int expected)
        {
            var state = new BinaryStreamReaderState(data);
            Assert.Equal(expected, state.CreateReader().ReadCompressedInt32());
            Assert.True(state.CreateReader().TryReadCompressedInt32(out int value));
            Assert.Equal(expected, value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello, world!")]
        [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
        public void ReadBinaryFormatterString(string value)
        {
            using var stream = new MemoryStream();
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(value);

            var reader = new BinaryStreamReader(stream);
            Assert.Equal(value, reader.ReadBinaryFormatterString());
        }

        [Fact]
        public void NewForkSubRange()
        {
            var reader = new BinaryStreamReader([
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            ]);

            var fork = reader.GetState().WithOffsetSize(2, 3).CreateReader();

            Assert.Equal(2u, fork.StartOffset);
            Assert.Equal(2u, fork.Offset);
            Assert.Equal(3u, fork.Length);
        }

        [Fact]
        public void NewForkInvalidStart()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var reader = new BinaryStreamReader([
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
                ]);

                reader.GetState().WithOffsetSize(9, 3);
            });
        }

        [Fact]
        public void NewForkTooLong()
        {
            Assert.Throws<EndOfStreamException>(() =>
            {
                var reader = new BinaryStreamReader([
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
                ]);

                reader.GetState().WithOffsetSize(6, 4);
            });
        }

        [Fact]
        public void ForkReadsSameData()
        {
            var reader = new BinaryStreamReader([
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            ]);

            var fork = reader.GetState().WithOffsetSize(0, 2).CreateReader();
            Assert.Equal(0x0201, fork.ReadUInt16());
        }

        [Fact]
        public void ForkMovesIndependentOfOriginal()
        {
            var reader = new BinaryStreamReader([
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            ]);

            var fork = reader.GetState().WithOffsetSize(0, 2).CreateReader();
            fork.ReadUInt16();

            Assert.Equal(0u, reader.Offset);
            Assert.Equal(2u, fork.Offset);
        }

        [Fact]
        public void ForkStartAtMiddle()
        {
            var reader = new BinaryStreamReader([
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            ]);

            var fork = reader.GetState().WithOffsetSize(4, 2).CreateReader();
            Assert.Equal(0x0605, fork.ReadUInt16());
        }

        [Fact]
        public void ForkOfFork()
        {
            var reader = new BinaryStreamReader([
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            ]);

            var fork = reader.GetState().WithOffsetSize(2, 4).CreateReader();
            var fork2 = fork.GetState().WithOffsetSize(3, 2).CreateReader();
            Assert.Equal(0x04, fork2.ReadByte());
        }
    }

}
