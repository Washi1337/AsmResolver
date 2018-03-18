using System;
using System.IO;

namespace AsmResolver
{
    public class MemoryStreamReader : IBinaryStreamReader
    {
        private readonly int _startOffset;
        private readonly int _endOffset;
        private int _position;
        private readonly byte[] _data;

        public MemoryStreamReader(byte[] data)
            : this(data, 0, data.Length)
        {
        }

        public MemoryStreamReader(byte[] data, int startOffset, int endOffset)
        {
            _data = data;
            _position = _startOffset = startOffset;
            _endOffset = endOffset;
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public long StartPosition
        {
            get { return _startOffset; }
        }

        public long Position
        {
            get { return _position; }
            set { _position = (int)value; }
        }

        public long Length
        {
            get { return _endOffset - _startOffset; }
        }

        public IBinaryStreamReader CreateSubReader(long address, int size)
        {
            return new MemoryStreamReader(_data, (int)address, (int)address + size);
        }

        private void Advance(int count)
        {
            if (_position > _endOffset)
                throw new EndOfStreamException();
            _position += count;
        }

        public byte[] ReadBytesUntil(byte value)
        {
            var index = Array.IndexOf(_data, value, _position);
            if (index == -1)
            {
                Position = Length;
                throw new EndOfStreamException();
            }
            return ReadBytes(index - _position);
        }

        public byte[] ReadBytes(int count)
        {
            Advance(count);
            var buffer = new byte[count];
            Buffer.BlockCopy(_data, _position - count, buffer, 0, count);
            return buffer;
        }

        public byte ReadByte()
        {
            Advance(1);
            return _data[_position - 1];
        }

        public ushort ReadUInt16()
        {
            Advance(2);
            return BitConverter.ToUInt16(_data, _position - 2);
        }

        public uint ReadUInt32()
        {
            Advance(4);
            return BitConverter.ToUInt32(_data, _position - 4);
        }

        public ulong ReadUInt64()
        {
            Advance(8);
            return BitConverter.ToUInt64(_data, _position - 8);
        }

        public sbyte ReadSByte()
        {
            Advance(1);
            return unchecked((sbyte)_data[_position - 1]);
        }

        public short ReadInt16()
        {
            Advance(2);
            return BitConverter.ToInt16(_data, _position - 2);
        }

        public int ReadInt32()
        {
            Advance(4);
            return BitConverter.ToInt32(_data, _position - 4);
        }

        public long ReadInt64()
        {
            Advance(8);
            return BitConverter.ToInt64(_data, _position - 8);
        }

        public float ReadSingle()
        {
            Advance(4);
            return BitConverter.ToSingle(_data, _position - 4);
        }

        public double ReadDouble()
        {
            Advance(8);
            return BitConverter.ToDouble(_data, _position - 8);
        }
    }
}
