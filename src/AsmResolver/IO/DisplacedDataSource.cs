using System;

namespace AsmResolver.IO
{
    /// <summary>
    /// Represents a data source that was moved in memory to a different address.
    /// </summary>
    public class DisplacedDataSource : IDataSource
    {
        private readonly IDataSource _dataSource;
        private readonly long _displacement;

        /// <summary>
        /// Creates a new displace data source.
        /// </summary>
        /// <param name="dataSource">The original data source that was moved.</param>
        /// <param name="displacement">The number of bytes the data source was shifted by.</param>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="dataSource"/> is <c>null</c>.</exception>
        public DisplacedDataSource(IDataSource dataSource, long displacement)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _displacement = displacement;
        }

        /// <inheritdoc />
        public ulong BaseAddress => _dataSource.BaseAddress + (ulong) _displacement;

        /// <inheritdoc />
        public byte this[ulong address] => _dataSource[address - (ulong) _displacement];

        /// <inheritdoc />
        public ulong Length => _dataSource.Length;

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => _dataSource.IsValidAddress(address - (ulong) _displacement);

        /// <inheritdoc />
        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            return _dataSource.ReadBytes(address - (ulong) _displacement, buffer, index, count);
        }
    }
}
