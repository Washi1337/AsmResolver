using System;
using System.IO;
using AsmResolver.Shims;

namespace AsmResolver.IO
{
    /// <summary>
    /// Represents an input file that is mapped in memory.
    /// </summary>
    public sealed class MemoryMappedInputFile : IInputFile
    {
        private readonly MemoryMappedFileShim _file;
        private readonly UnmanagedDataSource _dataSource;

        /// <summary>
        /// Creates a new reader factory for the provided file.
        /// </summary>
        /// <param name="filePath">The path to the file to read.</param>
        public unsafe MemoryMappedInputFile(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _file = new MemoryMappedFileShim(filePath);
            _dataSource = new UnmanagedDataSource(_file.BasePointer, (ulong)_file.Size);
        }

        /// <inheritdoc />
        public string FilePath
        {
            get;
        }

        /// <inheritdoc />
        public uint Length => (uint) _dataSource.Length;

        /// <inheritdoc />
        public unsafe BinaryStreamReader CreateReader(ulong address, uint rva, uint length) =>
            new(_dataSource, address != 0 ? address : (ulong)_file.BasePointer, rva, length);

        /// <inheritdoc />
        public void Dispose()
        {
            _file.Dispose();
        }
    }
}
