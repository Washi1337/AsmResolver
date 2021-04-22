using System.IO;
using System.IO.MemoryMappedFiles;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides a factory for binary stream readers that operate on a memory mapped file.
    /// </summary>
    public sealed class MemoryMappedReaderFactory : IBinaryStreamReaderFactory
    {
        private readonly MemoryMappedFile _file;
        private readonly MemoryMappedDataSource _dataSource;

        /// <summary>
        /// Creates a new reader factory for the provided file.
        /// </summary>
        /// <param name="filePath">The path to the file to read.</param>
        public MemoryMappedReaderFactory(string filePath)
        {
            _file = MemoryMappedFile.CreateFromFile(filePath);
            long fileSize = new FileInfo(filePath).Length;
            _dataSource = new MemoryMappedDataSource(_file.CreateViewAccessor(0, fileSize), (ulong) fileSize);
        }

        /// <inheritdoc />
        public uint MaxLength => (uint) _dataSource.Length;

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong address, uint rva, uint length) =>
            new(_dataSource, address, rva, length);

        /// <inheritdoc />
        public void Dispose()
        {
            _file?.Dispose();
            _dataSource?.Dispose();
        }
    }
}
