using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AsmResolver.IO;
using Xunit;

namespace AsmResolver.Tests.IO
{
    public class BinaryStreamReaderTest
    {
        private static IDataSource CreateByteDataSource(bool useSpan, byte[] data)
        {
            return useSpan ? new ByteArrayDataSource(data) : new NoSpanByteArrayDataSource(data);
        }

        [Theory]
        [MemberData(nameof(XunitHelpers.Bool1), MemberType = typeof(XunitHelpers))]
        public void EmptyArray(bool useSpan)
        {
            var readerState = new BinaryStreamReaderState(CreateByteDataSource(useSpan, []));
            Assert.Equal(0u, readerState.Length);

            Assert.Throws<EndOfStreamException>(() => readerState.CreateReader().ReadByte());
            Assert.Equal(0, readerState.CreateReader().ReadBytes(new byte[10], 0, 10));
        }

        [Theory]
        [MemberData(nameof(XunitHelpers.Bool1), MemberType = typeof(XunitHelpers))]
        public void ReadByte(bool useSpan)
        {
            var state = new BinaryStreamReaderState(CreateByteDataSource(useSpan, [0x80, 0x80]));
            var reader = state.CreateReader();

            Assert.Equal((byte) 0x80, reader.ReadByte());
            Assert.Equal(1u, reader.Offset);
            Assert.Equal((sbyte) -128, reader.ReadSByte());

            state = reader.GetState();
            Assert.ThrowsAny<Exception>(() => state.CreateReader().ReadByte());
        }

        [Theory]
        [MemberData(nameof(XunitHelpers.Bool1), MemberType = typeof(XunitHelpers))]
        public void ReadInt16(bool useSpan)
        {
            var state = new BinaryStreamReaderState(CreateByteDataSource(useSpan, [
                0x01, 0x80,
                0x02, 0x80
            ]));

            var reader = state.CreateReader();
            Assert.Equal((ushort) 0x8001, reader.ReadUInt16());
            Assert.Equal(2u, reader.Offset);
            Assert.Equal((short) -32766, reader.ReadInt16());
            Assert.Equal(4u, reader.Offset);
        }

        [Theory]
        [MemberData(nameof(XunitHelpers.Bool1), MemberType = typeof(XunitHelpers))]
        public void ReadInt32(bool useSpan)
        {
            var state = new BinaryStreamReaderState(CreateByteDataSource(useSpan, [
                0x04, 0x03, 0x02, 0x81,
                0x08, 0x07, 0x06, 0x85
            ]));

            var reader = state.CreateReader();
            Assert.Equal(0x81020304u, reader.ReadUInt32());
            Assert.Equal(4u, reader.Offset);
            Assert.Equal(-2063202552, reader.ReadInt32());
            Assert.Equal(8u, reader.Offset);
        }

        [Theory]
        [MemberData(nameof(XunitHelpers.Bool1), MemberType = typeof(XunitHelpers))]
        public void ReadInt64(bool useSpan)
        {
            var state = new BinaryStreamReaderState(CreateByteDataSource(useSpan, [
                0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x80,
                0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x88
            ]));

            var reader = state.CreateReader();
            Assert.Equal(0x8001020304050607ul, reader.ReadUInt64());
            Assert.Equal(8u, reader.Offset);
            Assert.Equal(-8644366967197856241, reader.ReadInt64());
            Assert.Equal(16u, reader.Offset);
        }

        public static IEnumerable<object[]> ReadCompressedUInt32Data() => XunitHelpers.Bool1().Cross([
            [new byte[] { 0x03 }, 3],
            [new byte[] { 0x7f }, 0x7f],
            [new byte[] { 0x80, 0x80 }, 0x80],
            [new byte[] { 0xAE, 0x57 }, 0x2E57],
            [new byte[] { 0xBF, 0xFF }, 0x3FFF],
            [new byte[] { 0xC0, 0x00, 0x40, 0x00 }, 0x4000],
            [new byte[] { 0xDF, 0x12, 0x34, 0x56 }, 0x1F123456],
            [new byte[] { 0xDF, 0xFF, 0xFF, 0xFF }, 0x1FFFFFFF]
        ]);

        [Theory]
        [MemberData(nameof(ReadCompressedUInt32Data))]
        public void ReadCompressedUInt32(bool useSpan, byte[] data, uint expected)
        {
            var state = new BinaryStreamReaderState(CreateByteDataSource(useSpan, data));
            Assert.Equal(expected, state.CreateReader().ReadCompressedUInt32());
            Assert.True(state.CreateReader().TryReadCompressedUInt32(out uint value));
            Assert.Equal(expected, value);
        }

        public static IEnumerable<object[]> ReadCompressedInt32Data() => XunitHelpers.Bool1().Cross([
            [new byte[] { 0x06 }, 3],
            [new byte[] { 0x7B }, -3],
            [new byte[] { 0x80, 0x80 }, 64],
            [new byte[] { 0x01 }, -64],
            [new byte[] { 0xC0, 0x00, 0x40, 0x00 }, 8192],
            [new byte[] { 0x80, 0x01 }, -8192],
            [new byte[] { 0xDF, 0xFF, 0xFF, 0xFE }, 0xFFFFFFF],
            [new byte[] { 0xC0, 0x00, 0x00, 0x01 }, -0x10000000]
        ]);

        [Theory]
        [MemberData(nameof(ReadCompressedInt32Data))]
        public void ReadCompressedInt32(bool useSpan, byte[] data, int expected)
        {
            var state = new BinaryStreamReaderState(CreateByteDataSource(useSpan, data));
            Assert.Equal(expected, state.CreateReader().ReadCompressedInt32());
            Assert.True(state.CreateReader().TryReadCompressedInt32(out int value));
            Assert.Equal(expected, value);
        }

        public static IEnumerable<object[]> ReadBinaryFormatterStringData() => XunitHelpers.Bool1().Cross([
            [""],
            ["Hello, world!"],
            ["0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789"]
        ]);

        [Theory]
        [MemberData(nameof(ReadBinaryFormatterStringData))]
        public void ReadBinaryFormatterString(bool useSpan, string value)
        {
            using var stream = new MemoryStream();
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(value);

            var reader = new BinaryStreamReader(CreateByteDataSource(useSpan, stream.ToArray()));
            Assert.Equal(value, reader.ReadBinaryFormatterString());
        }

        public static IEnumerable<object[]> AdvanceUntilData() => XunitHelpers.Bool1().Cross([
            ["1234"u8.ToArray(), 0x00, false, false, 4],
            ["1234"u8.ToArray(), 0x00, true, false, 4],
            ["1234\0"u8.ToArray(), 0x00, false, false, 4],
            ["1234\0"u8.ToArray(), 0x00, true, true, 5],
            ["1234\056"u8.ToArray(), 0x00, false, false, 4],
            ["1234\056"u8.ToArray(), 0x00, true, true, 5],
            ["123456"u8.ToArray(), 0x00, true, false, 6],
        ]);

        [Theory]
        [MemberData(nameof(AdvanceUntilData))]
        public void AdvanceUntil(bool useSpan, byte[] data, byte delimeter, bool consumeDelimiter, bool expectedConsumed, uint expectedOffset)
        {
            var reader = new BinaryStreamReader(CreateByteDataSource(useSpan, data));
            Assert.Equal(expectedConsumed, reader.AdvanceUntil(delimeter, consumeDelimiter));
            Assert.Equal(expectedOffset, reader.RelativeOffset);
        }

        public static IEnumerable<object[]> ReadBytesUntilData() => XunitHelpers.Bool1().Cross([
            ["1234"u8.ToArray(), 0x00, false, new byte[] { 0x31, 0x32, 0x33, 0x34 }],
            ["1234"u8.ToArray(), 0x00, true, new byte[] { 0x31, 0x32, 0x33, 0x34 }],
            ["1234\0"u8.ToArray(), 0x00, false, new byte[] { 0x31, 0x32, 0x33, 0x34 }],
            ["1234\0"u8.ToArray(), 0x00, true, new byte[] { 0x31, 0x32, 0x33, 0x34, 0x00 }],
            ["1234\056"u8.ToArray(), 0x00, false, new byte[] { 0x31, 0x32, 0x33, 0x34 }],
            ["1234\056"u8.ToArray(), 0x00, true, new byte[] { 0x31, 0x32, 0x33, 0x34, 0x00 }],
            ["123456"u8.ToArray(), 0x00, true, new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36 }],
        ]);

        [Theory]
        [MemberData(nameof(ReadBytesUntilData))]
        public void ReadBytesUntil(bool useSpan, byte[] data, byte delimeter, bool includeDelimiter, byte[] expectedData)
        {
            var reader = new BinaryStreamReader(CreateByteDataSource(useSpan, data));
            Assert.Equal(expectedData, reader.ReadBytesUntil(delimeter, includeDelimiter));
        }

        public static IEnumerable<object[]> ReadAsciiStringData() => XunitHelpers.Bool1().Cross([
            ["1234"u8.ToArray(), "1234", 4],
            ["1234\0"u8.ToArray(), "1234", 5],
            ["1234\056"u8.ToArray(), "1234", 5],
            ["123456"u8.ToArray(), "123456", 6],
        ]);

        [Theory]
        [MemberData(nameof(ReadAsciiStringData))]
        public void ReadAsciiString(bool useSpan, byte[] data, string expected, uint expectedOffset)
        {
            var reader = new BinaryStreamReader(CreateByteDataSource(useSpan, data));
            Assert.Equal(expected, reader.ReadAsciiString());
            Assert.Equal(expectedOffset, reader.RelativeOffset);
        }

        public static IEnumerable<object[]> ReadUnicodeStringData() => XunitHelpers.Bool1().Cross([
            ["1\02\03\04\0"u8.ToArray(), "1234", 8],
            ["1\02\03\04\0\0\0"u8.ToArray(), "1234", 10],
            ["1\02\03\04\0\0\05\06\0"u8.ToArray(), "1234", 10],
            ["1\02\03\04\05\06\0"u8.ToArray(), "123456", 12],
            ["1\02\03\04\05\06\0\0"u8.ToArray(), "123456", 12],
        ]);

        [Theory]
        [MemberData(nameof(ReadUnicodeStringData))]
        public void ReadUnicodeString(bool useSpan, byte[] data, string expected, uint expectedOffset)
        {
            var reader = new BinaryStreamReader(CreateByteDataSource(useSpan, data));
            Assert.Equal(expected, reader.ReadUnicodeString());
            Assert.Equal(expectedOffset, reader.RelativeOffset);
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
