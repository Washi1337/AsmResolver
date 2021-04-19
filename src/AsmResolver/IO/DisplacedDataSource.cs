namespace AsmResolver.IO
{
    public class DisplacedDataSource : IDataSource
    {
        private readonly IDataSource _dataSource;
        private readonly long _displacement;

        public DisplacedDataSource(IDataSource dataSource, long displacement)
        {
            _dataSource = dataSource;
            _displacement = displacement;
        }

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
