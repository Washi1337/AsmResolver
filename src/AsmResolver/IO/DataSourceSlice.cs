using System;

namespace AsmResolver.IO
{
    /// <summary>
    /// Represents a data source that only exposes a part (slice) of another data source.
    /// </summary>
    public class DataSourceSlice : IDataSource
    {
        private readonly IDataSource _source;

        /// <summary>
        /// Creates a new data source slice.
        /// </summary>
        /// <param name="source">The original data source to slice.</param>
        /// <param name="start">The starting address.</param>
        /// <param name="length">The number of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when <paramref name="start"/> and/or <paramref name="length"/> result in addresses that are invalid
        /// in the original data source.
        /// </exception>
        public DataSourceSlice(IDataSource source, ulong start, ulong length)
        {
            _source = source;

            if (!source.IsValidAddress(start))
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length > 0 && !source.IsValidAddress(start + length - 1))
                throw new ArgumentOutOfRangeException(nameof(length));

            BaseAddress = start;
            Length = length;
        }

        /// <inheritdoc />
        public ulong BaseAddress
        {
            get;
        }

        /// <inheritdoc />
        public ulong Length
        {
            get;
        }

        /// <inheritdoc />
        public byte this[ulong address]
        {
            get
            {
                if (!IsValidAddress(address))
                    throw new IndexOutOfRangeException();
                return _source[address];
            }
        }

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => address >= BaseAddress && address - BaseAddress < Length;

        /// <inheritdoc />
        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            int maxCount = Math.Max(0, (int) (Length - (address - BaseAddress)));
            return _source.ReadBytes(address, buffer, index, Math.Min(maxCount, count));
        }
    }
}
