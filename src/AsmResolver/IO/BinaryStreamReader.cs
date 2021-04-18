using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            private set;
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
            get => RelativeOffset + StartRva;
            set => RelativeOffset = value - StartRva;
        }

        public bool IsValid => DataSource is not null;

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

        public byte[] ReadBytesUntil(byte delimeter)
        {
            var buffer = new List<byte>();

            while (RelativeOffset < Length)
            {
                byte b = ReadByte();
                buffer.Add(b);
                if (b == delimeter)
                    break;
            }

            return buffer.ToArray();
        }

        public string ReadAsciiString()
        {
            byte[] data = ReadBytesUntil(0);
            int length = data.Length;

            // Exclude trailing 0 byte.
            if (data[data.Length - 1] == 0)
                length--;

            return Encoding.ASCII.GetString(data, 0, length);
        }

        public ulong ReadNativeInt(bool is32Bit)
        {
            return is32Bit ? ReadUInt32() : ReadUInt64();
        }

        /// <summary>
        /// Reads a compressed unsigned integer from the stream.
        /// </summary>
        /// <returns>The unsigned integer that was read from the stream.</returns>
        public uint ReadCompressedUInt32()
        {
            var firstByte = ReadByte();

            if ((firstByte & 0x80) == 0)
                return firstByte;

            if ((firstByte & 0x40) == 0)
                return (uint)(((firstByte & 0x7F) << 8) | ReadByte());

            return (uint) (((firstByte & 0x3F) << 0x18) |
                           (ReadByte() << 0x10) |
                           (ReadByte() << 0x08) |
                           ReadByte());
        }

        /// <summary>
        /// Tries to reads a compressed unsigned integer from the stream.
        /// </summary>
        /// <param name="value">The unsigned integer that was read from the stream.</param>
        /// <returns><c>True</c> if the method succeeded, false otherwise.</returns>
        public bool TryReadCompressedUInt32(out uint value)
        {
            value = 0;
            if (!CanRead(sizeof(byte)))
                return false;

            var firstByte = ReadByte();
            Offset--;

            if (((firstByte & 0x80) == 0 && CanRead(sizeof(byte))) ||
                ((firstByte & 0x40) == 0 && CanRead(sizeof(ushort))) ||
                (CanRead(sizeof(uint))))
            {
                value = ReadCompressedUInt32();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads a short or a long index from the stream.
        /// </summary>
        /// <param name="size">The size of the index to read.</param>
        /// <returns>The index, zero extended to 32 bits if necessary.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public uint ReadIndex(IndexSize size)
        {
            switch (size)
            {
                case IndexSize.Short:
                    return ReadUInt16();
                case IndexSize.Long:
                    return ReadUInt32();
                default:
                    throw new ArgumentOutOfRangeException(nameof(size));
            }
        }

        /// <summary>
        /// Reads a serialized UTF8 string from the stream.
        /// </summary>
        /// <returns>The string that was read from the stream.</returns>
        public string ReadSerString()
        {
            if (!CanRead(1) || ReadByte() == 0xFF)
                return null;
            Offset--;
            if (!TryReadCompressedUInt32(out uint length))
                return null;
            var data = new byte[length];
            length = (uint) ReadBytes(data, 0, (int) length);
            return Encoding.UTF8.GetString(data, 0, (int) length);
        }

        /// <summary>
        /// Aligns the reader to a specified boundary.
        /// </summary>
        /// <param name="alignment">The boundary to use.</param>
        public void Align(uint alignment)
        {
            Offset = Offset.Align(alignment);
        }

        public readonly BinaryStreamReader Fork() => this;

        public readonly BinaryStreamReader ForkAbsolute(ulong offset)
        {
            return ForkAbsolute(offset, (uint) (Length - (offset - StartOffset)));
        }

        public readonly BinaryStreamReader ForkAbsolute(ulong offset, uint size)
        {
            return new(DataSource, offset, (uint) (StartRva + (offset - StartOffset)), size);
        }

        public void ChangeSize(uint newSize)
        {
            if (newSize > Length)
                throw new EndOfStreamException();

            Length = newSize;
        }
    }
}
