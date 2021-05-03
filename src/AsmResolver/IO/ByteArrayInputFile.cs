using System;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides a mechanism to build binary stream readers from byte arrays.
    /// </summary>
    public sealed class ByteArrayInputFile : IInputFile
    {
        private readonly ByteArrayDataSource _dataSource;

        /// <summary>
        /// Creates a new reader factory for the provided byte array.
        /// </summary>
        /// <param name="data">The byte array to read from.</param>
        /// <param name="baseAddress">The base address to use.</param>
        public ByteArrayInputFile(string filePath, byte[] data, ulong baseAddress)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _dataSource = new ByteArrayDataSource(data, baseAddress);
        }

        /// <inheritdoc />
        public string FilePath
        {
            get;
        }

        /// <inheritdoc />
        public uint Length => (uint) _dataSource.Length;

        /// <summary>
        /// Constructs a new binary stream reader on the provided byte array.
        /// </summary>
        /// <param name="data">The byte array to read.</param>
        /// <returns>The stream reader.</returns>
        public static BinaryStreamReader CreateReader(byte[] data) =>
            new(new ByteArrayDataSource(data), 0, 0, (uint) data.Length);

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong address, uint rva, uint length) =>
            new(_dataSource, address, rva, length);

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
        }
    }
}
