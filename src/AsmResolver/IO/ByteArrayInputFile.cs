using System;
using System.IO;

namespace AsmResolver.IO
{
    /// <summary>
    /// Represents a file for which the data is represented by a byte array.
    /// </summary>
    public sealed class ByteArrayInputFile : IInputFile
    {
        private readonly ByteArrayDataSource _dataSource;

        /// <summary>
        /// Creates a new file for the provided file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public ByteArrayInputFile(string filePath)
            : this(filePath, File.ReadAllBytes(filePath), 0)
        {
        }

        /// <summary>
        /// Creates a new file for the provided raw contents.
        /// </summary>
        /// <param name="contents">The raw contents of the file.</param>
        public ByteArrayInputFile(byte[] contents)
            : this(null, contents, 0)
        {
        }

        /// <summary>
        /// Creates a new file for the provided file path and raw contents.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="data">The byte array to read from.</param>
        /// <param name="baseAddress">The base address to use.</param>
        public ByteArrayInputFile(string filePath, byte[] data, ulong baseAddress)
        {
            FilePath = filePath;
            _dataSource = new ByteArrayDataSource(data, baseAddress);
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
        void IDisposable.Dispose()
        {
        }
    }
}
