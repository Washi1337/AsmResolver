using System;

namespace AsmResolver.IO
{
    public sealed class ByteArrayReaderFactory : IBinaryStreamReaderFactory
    {
        private readonly ByteArrayDataSource _dataSource;

        public ByteArrayReaderFactory(byte[] data, ulong baseAddress)
        {
            _dataSource = new ByteArrayDataSource(data, baseAddress);
        }

        public static BinaryStreamReader CreateReader(byte[] data) =>
            new ByteArrayReaderFactory(data, 0).CreateReader(0, 0, (uint) data.Length);

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong address, uint rva, uint length) => new(_dataSource, address, rva, length);

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
        }
    }
}
