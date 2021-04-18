using System;
using System.IO;

namespace AsmResolver.IO
{
    public struct BinaryStreamReader
    {
        public BinaryStreamReader(IDataSource dataSource, ulong offset, uint rva, uint length)
        {
            if (dataSource is null)
                throw new ArgumentNullException(nameof(dataSource));
            
            if (length > 0)
            {
                if (!dataSource.IsValidAddress(offset))
                    throw new ArgumentOutOfRangeException(nameof(offset));
                if (!dataSource.IsValidAddress(offset + length - 1))
                    throw new EndOfStreamException(
                        "Offset and address reach outside of the boundaries of the data source.");
            }

            DataSource = dataSource;
            StartOffset = Offset = offset;
            StartRva = rva;
            Length = length;
        }

        public IDataSource DataSource
        {
            get;
        }

        public ulong StartOffset
        {
            get;
        }

        public uint StartRva
        {
            get;
        }

        public uint Length
        {
            get;
        }

        public ulong EndOffset => StartOffset + Length;

        public ulong EndRva => StartRva + Length;

        public ulong Offset
        {
            get;
            set;
        }

        public uint RelativeOffset
        {
            get => (uint) (Offset - StartOffset);
            set => Offset = value + StartOffset;
        }

        public uint Rva
        {
            get => (uint) (Offset - StartOffset + StartRva);
            set => Offset = value - StartRva + StartOffset;
        }

        public bool CanRead(uint count) => RelativeOffset + count <= Length;

        private void AssertCanRead(uint count)
        {
            if (!CanRead(count))
                throw new EndOfStreamException();
        }

        public byte ReadByte()
        {
            AssertCanRead(sizeof(byte));
            return DataSource[Offset++];
        }

        public ushort ReadUInt16()
        {
            AssertCanRead(2);
            ushort value = (ushort) (DataSource[Offset]
                                     | (DataSource[Offset + 1] << 8));
            Offset += 2;
            return value;
        }

        public uint ReadUInt32()
        {
            AssertCanRead(4);
            uint value = unchecked((uint) (DataSource[Offset]
                                           | (DataSource[Offset + 1] << 8)
                                           | (DataSource[Offset + 2] << 16)
                                           | (DataSource[Offset + 3] << 24)));
            Offset += 4;
            return value;
        }

        public ulong ReadUInt64()
        {
            AssertCanRead(8);
            ulong value = unchecked((ulong) (DataSource[Offset]
                                             | ( (long) DataSource[Offset + 1] << 8)
                                             | ( (long) DataSource[Offset + 2] << 16)
                                             | ( (long) DataSource[Offset + 3] << 24)
                                             | ( (long) DataSource[Offset + 4] << 32)
                                             | ( (long) DataSource[Offset + 5] << 40)
                                             | ( (long) DataSource[Offset + 6] << 48)
                                             | ( (long) DataSource[Offset + 7] << 56)));
            Offset += 8;
            return value;
        }

        public sbyte ReadSByte()
        {
            AssertCanRead(1);
            return unchecked((sbyte) DataSource[Offset++]);
        }

        public short ReadInt16()
        {
            AssertCanRead(2);
            short value = (short) (DataSource[Offset]
                                   | (DataSource[Offset + 1] << 8));
            Offset += 2;
            return value;
        }

        public int ReadInt32()
        {
            AssertCanRead(4);
            int value = DataSource[Offset]
                        | (DataSource[Offset + 1] << 8)
                        | (DataSource[Offset + 2] << 16)
                        | (DataSource[Offset + 3] << 24);
            Offset += 4;
            return value;
        }

        public long ReadInt64()
        {
            AssertCanRead(8);
            long value = (DataSource[Offset]
                          | ((long) DataSource[Offset + 1] << 8)
                          | ((long) DataSource[Offset + 2] << 16)
                          | ((long) DataSource[Offset + 3] << 24)
                          | ((long) DataSource[Offset + 4] << 32)
                          | ((long) DataSource[Offset + 5] << 40)
                          | ((long) DataSource[Offset + 6] << 48)
                          | ((long) DataSource[Offset + 7] << 56));
            Offset += 8;
            return value;
        }

        public unsafe float ReadSingle()
        {
            uint raw = ReadUInt32();
            return *(float*) &raw;
        }

        public unsafe double ReadDouble()
        {
            ulong raw = ReadUInt64();
            return *(double*) &raw;
        }

        public int ReadBytes(byte[] buffer, int index, int count)
        {
            int actualLength = DataSource.ReadBytes(Offset, buffer, index, count);
            Offset += (uint) actualLength;
            return actualLength;
        }

        public byte[] ReadToEnd()
        {
            byte[] buffer = new byte[Length - RelativeOffset];
            ReadBytes(buffer, 0, buffer.Length);
            return buffer;
        }

        public BinaryStreamReader Fork() => this;

        public BinaryStreamReader ForkAbsolute(ulong offset)
        {
            return ForkAbsolute(offset, (uint) (Length - (offset - StartOffset)));
        }

        public BinaryStreamReader ForkAbsolute(ulong offset, uint size)
        {
            return new(DataSource, offset, (uint) (StartRva + (offset - StartOffset)), size);
        }
    }
}
