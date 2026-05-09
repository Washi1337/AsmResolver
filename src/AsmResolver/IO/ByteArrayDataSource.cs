using System;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides a <see cref="IDataSource"/> wrapper around a raw byte array.
    /// </summary>
    public sealed class ByteArrayDataSource : IDataSource
    {
        private readonly byte[] _data;

        /// <summary>
        /// Creates a new instance of the <see cref="ByteArrayDataSource"/> class.
        /// </summary>
        /// <param name="data">The raw data to read from.</param>
        public ByteArrayDataSource(byte[] data)
            : this(data, 0)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ByteArrayDataSource"/> class.
        /// </summary>
        /// <param name="data">The raw data to read from.</param>
        /// <param name="baseAddress">The base address to use.</param>
        public ByteArrayDataSource(byte[] data, ulong baseAddress)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            BaseAddress = baseAddress;
        }

        /// <inheritdoc />
        public ulong BaseAddress
        {
            get;
        }

        /// <inheritdoc />
        public byte this[ulong address] => _data[address - BaseAddress];

        /// <inheritdoc />
        public ulong Length => (ulong) _data.Length;

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => address - BaseAddress < (ulong) _data.Length;

        /// <inheritdoc />
        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            int relativeIndex = (int) (address - BaseAddress);
            int actualLength = Math.Min(count, _data.Length - relativeIndex);
            Buffer.BlockCopy(_data, relativeIndex, buffer, index, actualLength);
            return actualLength;
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <inheritdoc />
        public void ReadBytes(ulong address, Span<byte> buffer)
        {
            _data.AsSpan((int) (address - BaseAddress), buffer.Length).CopyTo(buffer);
        }

        /// <inheritdoc />
        public bool TryGetSpan(ulong address, int length, out ReadOnlySpan<byte> span)
        {
            int relativeIndex = unchecked((int) (address - BaseAddress));
            if (relativeIndex < 0 || relativeIndex >= _data.Length - length)
            {
                span = default;
                return false;
            }

            span = _data;
            return true;
        }
#endif
    }
}
