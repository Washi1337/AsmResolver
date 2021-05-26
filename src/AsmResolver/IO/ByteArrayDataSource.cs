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

        /// <summary>
        /// Constructs a new binary stream reader on the provided byte array.
        /// </summary>
        /// <param name="data">The byte array to read.</param>
        /// <returns>The stream reader.</returns>
        public static BinaryStreamReader CreateReader(byte[] data) =>
            new(new ByteArrayDataSource(data), 0, 0, (uint) data.Length);

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => address - BaseAddress < (ulong) _data.Length;

        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            int relativeIndex = (int) (address - BaseAddress);
            int actualLength = Math.Min(count, _data.Length - relativeIndex);
            Buffer.BlockCopy(_data, relativeIndex, buffer, index, actualLength);
            return actualLength;
        }
    }
}
