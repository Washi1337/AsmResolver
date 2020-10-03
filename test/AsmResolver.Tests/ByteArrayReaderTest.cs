using System;
using System.IO;
using Xunit;

namespace AsmResolver.Tests
{
    public class ByteArrayReaderTest
    {
        [Fact]
        public void EmptyArray()
        {
            var reader = new ByteArrayReader(new byte[0]);
            Assert.Equal(0u, reader.Length);

            Assert.Throws<EndOfStreamException>(() => reader.ReadByte());
            Assert.Equal(0, reader.ReadBytes(new byte[10], 0, 10));
        }

        [Fact]
        public void ReadByte()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x80,
                0x80
            });

            Assert.Equal((byte) 0x80, reader.ReadByte());
            Assert.Equal(1u, reader.Offset);
            Assert.Equal((sbyte) -128, reader.ReadSByte());

            Assert.Throws<EndOfStreamException>(() => reader.ReadByte());
        }

        [Fact]
        public void ReadInt16()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x01, 0x80,
                0x02, 0x80
            });

            Assert.Equal((ushort) 0x8001, reader.ReadUInt16());
            Assert.Equal(2u, reader.Offset);
            Assert.Equal((short) -32766, reader.ReadInt16());
            Assert.Equal(4u, reader.Offset);
        }

        [Fact]
        public void ReadInt32()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x04, 0x03, 0x02, 0x81,
                0x08, 0x07, 0x06, 0x85
            });

            Assert.Equal(0x81020304u, reader.ReadUInt32());
            Assert.Equal(4u, reader.Offset);
            Assert.Equal(-2063202552, reader.ReadInt32());
            Assert.Equal(8u, reader.Offset);
        }


        [Fact]
        public void ReadInt64()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x80,
                0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x88,
            });

            Assert.Equal(0x8001020304050607ul, reader.ReadUInt64());
            Assert.Equal(8u, reader.Offset);
            Assert.Equal(-8644366967197856241, reader.ReadInt64());
            Assert.Equal(16u, reader.Offset);
        }

        [Fact]
        public void NewForkSubRange()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            });

            var fork = reader.Fork(2, 3);
            
            Assert.Equal(2u, fork.StartOffset);
            Assert.Equal(2u, fork.Offset);
            Assert.Equal(3u, fork.Length);
        }

        [Fact]
        public void NewForkInvalidStart()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            });

            Assert.Throws<ArgumentOutOfRangeException>(() => reader.Fork(8, 3));
        }

        [Fact]
        public void NewForkTooLong()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            });

            Assert.Throws<EndOfStreamException>(() => reader.Fork(6, 4));
        }

        [Fact]
        public void ForkReadsSameData()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            });
            
            var fork = reader.Fork(0, 2);
            Assert.Equal(0x0201, fork.ReadUInt16());
        }
        
        [Fact]
        public void ForkMovesIndependentOfOriginal()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            });
            
            var fork = reader.Fork(0, 2);
            fork.ReadUInt16();
                
            Assert.Equal(0u, reader.Offset);
            Assert.Equal(2u, fork.Offset);
        }

        [Fact]
        public void ForkStartAtMiddle()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            });
            
            var fork = reader.Fork(4, 2);
            Assert.Equal(0x0605, fork.ReadUInt16());
        }

        [Fact]
        public void ForkOfFork()
        {
            var reader = new ByteArrayReader(new byte[]
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
            });
            
            var fork = reader.Fork(2, 4);
            var fork2 = fork.Fork(3, 2);
            Assert.Equal(0x04, fork2.ReadByte());
        }
    }

}