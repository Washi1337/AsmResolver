using System;
using System.IO;

namespace AsmResolver
{
    /// <summary>
    /// Provides an implementation of a binary stream reader that uses a (chunk of a) byte array as a data source.
    /// </summary>
    public class ByteArrayReader : IBinaryStreamReader
    {
        private readonly byte[] _data;
        private readonly int _startIndex;
        private int _index;

        /// <summary>
        /// Creates a new byte array reader that spans the entire array.
        /// </summary>
        /// <param name="data">The data source.</param>
        public ByteArrayReader(byte[] data)
            : this(data, 0, (uint) data.Length, 0, 0)
        {
        }

        /// <summary>
        /// Creates a new byte array reader that reads a chunk of the data source.
        /// </summary>
        /// <param name="data">The data source.</param>
        /// <param name="index">The starting index of the chunk to read.</param>
        /// <param name="length">The number of bytes the chunk will consist of at most.</param>
        /// <param name="startFileOffset">The starting file offset of the chunk to read.</param>
        /// <param name="startRva">The starting rva of the chunk to read.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when <paramref name="index"/> is outside of the array.</exception>
        /// <exception cref="EndOfStreamException">Occurs when the reader reaches outside of the data source.</exception>
        public ByteArrayReader(byte[] data, int index, uint length, uint startFileOffset, uint startRva)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            if (index > data.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index + length > data.Length)
                throw new EndOfStreamException();
            _startIndex = _index = index;
            StartPosition = startFileOffset;
            StartRva = startRva;
            Length = length;
        }

        /// <inheritdoc />
        public uint StartPosition
        {
            get;
        }

        /// <inheritdoc />
        public uint StartRva
        {
            get;
        }

        /// <inheritdoc />
        public uint FileOffset
        {
            get => (uint) (_index - _startIndex + StartPosition);
            set => _index = (int) (value - StartPosition + _startIndex);
        }

        /// <inheritdoc />
        public uint Rva => (uint) (_index - _startIndex + StartRva);

        /// <inheritdoc />
        public uint Length
        {
            get;
            private set;
        }

        private void AssertCanRead(int count)
        {
            if (!this.CanRead(count))
                throw new EndOfStreamException();
        }

        /// <inheritdoc />
        public IBinaryStreamReader Fork(uint address, uint size)
        {
            if (address < StartPosition || address >= StartPosition + Length)
                throw new ArgumentOutOfRangeException(nameof(address));
            return new ByteArrayReader(
                _data, 
                (int) (address - StartPosition + _startIndex), 
                size,
                address,
                address - StartPosition + StartRva);
        }

        /// <inheritdoc />
        public void ChangeSize(uint newSize)
        {
            if (newSize > Length)
                throw new EndOfStreamException();
            
            Length = newSize;
        }

        /// <inheritdoc />
        public byte[] ReadBytesUntil(byte value)
        {
            int index = Array.IndexOf(_data, value, _index);
            if (index == -1)
                index = (int) (Length - 1);

            var data = new byte[index - _index + 1];
            ReadBytes(data, 0, data.Length);
            return data;
        }

        /// <inheritdoc />
        public int ReadBytes(byte[] buffer, int startIndex, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            
            count = (int) Math.Min(count, (StartPosition + Length) - FileOffset);
            Buffer.BlockCopy(_data, _index, buffer, startIndex, count);
            _index += count;
            
            return count;
        }

        /// <inheritdoc />
        public byte ReadByte()
        {
            AssertCanRead(1);
            return _data[_index++];
        }

        /// <inheritdoc />
        public ushort ReadUInt16()
        {
            AssertCanRead(2);
            ushort value = (ushort) (_data[_index]
                                     | (_data[_index + 1] << 8));
            _index += 2;
            return value;
        }

        /// <inheritdoc />
        public uint ReadUInt32()
        {
            AssertCanRead(4);
            uint value = unchecked((uint) (_data[_index]
                                           | (_data[_index + 1] << 8)
                                           | (_data[_index + 2] << 16)
                                           | (_data[_index + 3] << 24)));
            _index += 4;
            return value;
        }

        /// <inheritdoc />
        public ulong ReadUInt64()
        {
            AssertCanRead(8);
            ulong value = unchecked((ulong) (_data[_index]
                                             | ( (long) _data[_index + 1] << 8)
                                             | ( (long) _data[_index + 2] << 16)
                                             | ( (long) _data[_index + 3] << 24)
                                             | ( (long) _data[_index + 4] << 32)
                                             | ( (long) _data[_index + 5] << 40)
                                             | ( (long) _data[_index + 6] << 48)
                                             | ( (long) _data[_index + 7] << 56)));
            _index += 8;
            return value;
        }

        /// <inheritdoc />
        public sbyte ReadSByte()
        {
            AssertCanRead(1);
            return unchecked((sbyte) _data[_index++]);
        }

        /// <inheritdoc />
        public short ReadInt16()
        {
            AssertCanRead(2);
            short value = (short) (_data[_index]
                                   | (_data[_index + 1] << 8));
            _index += 2;
            return value;
        }

        /// <inheritdoc />
        public int ReadInt32()
        {
            AssertCanRead(4);
            int value = _data[_index]
                        | (_data[_index + 1] << 8)
                        | (_data[_index + 2] << 16)
                        | (_data[_index + 3] << 24);
            _index += 4;
            return value;
        }

        /// <inheritdoc />
        public long ReadInt64()
        {
            AssertCanRead(8);
            long value = (_data[_index]
                          | ((long) _data[_index + 1] << 8)
                          | ((long) _data[_index + 2] << 16)
                          | ((long) _data[_index + 3] << 24)
                          | ((long) _data[_index + 4] << 32)
                          | ((long) _data[_index + 5] << 40)
                          | ((long) _data[_index + 6] << 48)
                          | ((long) _data[_index + 7] << 56));
            _index += 8;
            return value;
        }

        /// <inheritdoc />
        public unsafe float ReadSingle()
        {
            uint raw = ReadUInt32();
            return *(float*) &raw;
        }

        /// <inheritdoc />
        public unsafe double ReadDouble()
        {
            ulong raw = ReadUInt64();
            return *(double*) &raw;
        }
        
    }
}