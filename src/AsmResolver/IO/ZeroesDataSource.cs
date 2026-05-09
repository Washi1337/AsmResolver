using System;
using System.IO;

namespace AsmResolver.IO
{
    /// <summary>
    /// Implements a data source that reads zero bytes.
    /// </summary>
    public sealed class ZeroesDataSource : IDataSource
    {
        /// <summary>
        /// Creates a new zeroes data source.
        /// </summary>
        /// <param name="length">The number of zero bytes.</param>
        public ZeroesDataSource(ulong length)
            : this(0, length)
        {
        }

        /// <summary>
        /// Creates a new zeroes data source.
        /// </summary>
        /// <param name="baseAddress">The base address of the segment.</param>
        /// <param name="length">The number of zero bytes.</param>
        public ZeroesDataSource(ulong baseAddress, ulong length)
        {
            BaseAddress = baseAddress;
            Length = length;
        }

        /// <inheritdoc />
        public ulong BaseAddress
        {
            get;
        }

        /// <inheritdoc />
        public byte this[ulong address] => !IsValidAddress(address)
            ? throw new IndexOutOfRangeException(nameof(address))
            : (byte) 0;

        /// <inheritdoc />
        public ulong Length
        {
            get;
        }

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => address - BaseAddress < Length;

        /// <inheritdoc />
        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            int actualLength = (int) Math.Min(Length, (ulong) count);
            Array.Clear(buffer, index, actualLength);
            return actualLength;
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <inheritdoc />
        public void ReadBytes(ulong address, Span<byte> buffer)
        {
            if (!IsValidAddress(address))
                throw new ArgumentOutOfRangeException(nameof(address));
            if (!IsValidAddress(address + (ulong) buffer.Length))
                throw new EndOfStreamException();

            buffer.Clear();
        }

        /// <inheritdoc />
        public bool TryGetSpan(ulong address, int length, out ReadOnlySpan<byte> span)
        {
            span = default;
            return false;
        }
#endif
    }
}
