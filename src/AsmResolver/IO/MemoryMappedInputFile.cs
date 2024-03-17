#if NETSTANDARD2_0_OR_GREATER

using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace AsmResolver.IO
{
    /// <summary>
    /// Represents an input file that is mapped in memory.
    /// </summary>
    public sealed class MemoryMappedInputFile : IInputFile
    {
        private readonly MemoryMappedFile _file;
        private readonly MemoryMappedDataSource _dataSource;

        /// <summary>
        /// Creates a new reader factory for the provided file.
        /// </summary>
        /// <param name="filePath">The path to the file to read.</param>
        public MemoryMappedInputFile(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _file = MemoryMappedFile.CreateFromFile(filePath);
            long fileSize = new FileInfo(filePath).Length;
            _dataSource = new MemoryMappedDataSource(_file.CreateViewAccessor(0, fileSize), (ulong) fileSize);
        }

        /// <inheritdoc />
        public string FilePath
        {
            get;
        }

        /// <inheritdoc />
        public uint Length => (uint) _dataSource.Length;

        /// <inheritdoc />
        public BinaryStreamReader CreateReader(ulong address, uint rva, uint length) =>
            new(_dataSource, address, rva, length);

        /// <inheritdoc />
        public void Dispose()
        {
            _file.Dispose();
            _dataSource.Dispose();
        }
    }
}

#endif
