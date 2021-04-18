using System;

namespace AsmResolver.IO
{
    public sealed class ByteArrayDataSource : IDataSource
    {
        private readonly byte[] _data;
        private readonly ulong _baseAddress;

        public ByteArrayDataSource(byte[] data)
            : this(data, 0)
        {
        }

        public ByteArrayDataSource(byte[] data, ulong baseAddress)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _baseAddress = baseAddress;
        }

        /// <inheritdoc />
        public byte this[ulong address] => _data[address - _baseAddress];

        /// <inheritdoc />
        public ulong Length => (ulong) _data.Length;

        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            int relativeIndex = (int) (address - _baseAddress);
            int actualLength = Math.Min(count, _data.Length - relativeIndex);
            Buffer.BlockCopy(_data, relativeIndex, buffer, index, actualLength);
            return actualLength;
        }
    }
}
