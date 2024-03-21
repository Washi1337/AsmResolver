using System;
using AsmResolver.Shims;

namespace AsmResolver.IO
{
    /// <summary>
    /// Represents a data source that obtains its data from a memory mapped file.
    /// </summary>
    public sealed class MemoryMappedDataSource : IDataSource, IDisposable
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        , ISpanDataSource
#endif
    {
        private readonly MemoryMappedFileShim _file;

        /// <summary>
        /// Creates a new instance of the <see cref="MemoryMappedDataSource"/> class.
        /// </summary>
        /// <param name="file">The memory mapped file to use.</param>
        internal MemoryMappedDataSource(MemoryMappedFileShim file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        /// <inheritdoc />
        public ulong BaseAddress => 0;

        /// <inheritdoc />
        public byte this[ulong address] => _file.ReadByte((long) address);

        /// <inheritdoc />
        public ulong Length => (ulong)_file.Size;

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => address < Length;

        /// <inheritdoc />
        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            if (address >= Length)
                throw new ArgumentOutOfRangeException(nameof(address));
            int actualLength = (int)Math.Min(Length - address, (ulong)count);
            _file.GetSpan((long)address, actualLength).CopyTo(buffer.AsSpan(index));
            return actualLength;
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <inheritdoc />
        public unsafe int ReadBytes(ulong address, Span<byte> buffer)
        {
            if (!IsValidAddress(address))
                return 0;

            int actualLength = (int) Math.Min(Length - address, (ulong)buffer.Length);

            _file.GetSpan((long)address, actualLength).CopyTo(buffer);

            return actualLength;
        }
#endif

        /// <inheritdoc />
        public void Dispose() => _file.Dispose();
    }
}
