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
        private long _position;

        /// <summary>
        /// Creates a new byte array reader that spans the entire array.
        /// </summary>
        /// <param name="data">The data source.</param>
        public ByteArrayReader(byte[] data)
            : this(data, 0L, data.Length)
        {
        }

        /// <summary>
        /// Creates a new byte array reader that reads a chunk of the data source.
        /// </summary>
        /// <param name="data">The data source.</param>
        /// <param name="start">The starting index of the chunk to read.</param>
        /// <param name="length">The number of bytes the chunk will consist of at most.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when <paramref name="start"/> is outside of the array.</exception>
        /// <exception cref="EndOfStreamException">Occurs when the reader reaches outside of the data source.</exception>
        private ByteArrayReader(byte[] data, long start, int length)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            if (start > data.Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (start + length > data.Length)
                throw new EndOfStreamException();
            StartPosition = start;
            Length = length;
            _position = start;
        }

        /// <inheritdoc />
        public long StartPosition
        {
            get;
        }

        /// <inheritdoc />
        public long Position
        {
            get => _position;
            set => _position = value;
        }

        /// <inheritdoc />
        public long Length
        {
            get;
        }

        private void AssertCanRead(int count)
        {
            if (!this.CanRead(count))
                throw new EndOfStreamException();
        }

        /// <inheritdoc />
        public IBinaryStreamReader Fork(long address, int size)
        {
            if (address < StartPosition || address >= StartPosition + Length)
                throw new ArgumentOutOfRangeException(nameof(address));
            return new ByteArrayReader(_data, address, size);
        }

        /// <inheritdoc />
        public byte[] ReadBytesUntil(byte value)
        {
            int index = Array.IndexOf(_data, value, (int) _position);
            if (index == -1)
                index = (int) (Length - 1);

            var data = new byte[index - _position + 1];
            ReadBytes(data, 0, data.Length);
            return data;
        }

        /// <inheritdoc />
        public int ReadBytes(byte[] buffer, int startIndex, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            
            count = (int) Math.Min(count, (StartPosition + Length) - Position);
            Buffer.BlockCopy(_data, (int) _position, buffer, startIndex, count);
            return count;
        }

        /// <inheritdoc />
        public byte ReadByte()
        {
            AssertCanRead(1);
            return _data[_position++];
        }

        /// <inheritdoc />
        public ushort ReadUInt16()
        {
            AssertCanRead(2);
            ushort value = (ushort) (_data[_position]
                                     | (_data[_position + 1] << 8));
            _position += 2;
            return value;
        }

        /// <inheritdoc />
        public uint ReadUInt32()
        {
            AssertCanRead(4);
            uint value = unchecked((uint) (_data[_position]
                                           | (_data[_position + 1] << 8)
                                           | (_data[_position + 2] << 16)
                                           | (_data[_position + 3] << 24)));
            _position += 4;
            return value;
        }

        /// <inheritdoc />
        public ulong ReadUInt64()
        {
            AssertCanRead(8);
            ulong value = unchecked((ulong) (_data[_position]
                                             | ( (long) _data[_position + 1] << 8)
                                             | ( (long) _data[_position + 2] << 16)
                                             | ( (long) _data[_position + 3] << 24)
                                             | ( (long) _data[_position + 4] << 32)
                                             | ( (long) _data[_position + 5] << 40)
                                             | ( (long) _data[_position + 6] << 48)
                                             | ( (long) _data[_position + 7] << 56)));
            _position += 8;
            return value;
        }

        /// <inheritdoc />
        public sbyte ReadSByte()
        {
            AssertCanRead(1);
            return unchecked((sbyte) _data[_position++]);
        }

        /// <inheritdoc />
        public short ReadInt16()
        {
            AssertCanRead(2);
            short value = (short) (_data[_position]
                                   | (_data[_position + 1] << 8));
            _position += 2;
            return value;
        }

        /// <inheritdoc />
        public int ReadInt32()
        {
            AssertCanRead(4);
            int value = _data[_position]
                        | (_data[_position + 1] << 8)
                        | (_data[_position + 2] << 16)
                        | (_data[_position + 3] << 24);
            _position += 4;
            return value;
        }

        /// <inheritdoc />
        public long ReadInt64()
        {
            AssertCanRead(8);
            long value = (_data[_position]
                          | ((long) _data[_position + 1] << 8)
                          | ((long) _data[_position + 2] << 16)
                          | ((long) _data[_position + 3] << 24)
                          | ((long) _data[_position + 4] << 32)
                          | ((long) _data[_position + 5] << 40)
                          | ((long) _data[_position + 6] << 48)
                          | ((long) _data[_position + 7] << 56));
            _position += 8;
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