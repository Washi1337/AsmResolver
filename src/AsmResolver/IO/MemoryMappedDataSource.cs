using System;
using System.IO.MemoryMappedFiles;

namespace AsmResolver.IO
{
    /// <summary>
    /// Represents a data source that obtains its data from a memory mapped file.
    /// </summary>
    public sealed class MemoryMappedDataSource : IDataSource, IDisposable
    {
        private readonly MemoryMappedViewAccessor _accessor;

        /// <summary>
        /// Creates a new instance of the <see cref="MemoryMappedDataSource"/> class.
        /// </summary>
        /// <param name="accessor">The memory accessor to use.</param>
        /// <param name="length">The length of the data.</param>
        public MemoryMappedDataSource(MemoryMappedViewAccessor accessor, ulong length)
        {
            Length = length;
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <inheritdoc />
        public ulong BaseAddress => 0;

        /// <inheritdoc />
        public byte this[ulong address] => _accessor.ReadByte((long) address);

        /// <inheritdoc />
        public ulong Length
        {
            get;
        }

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => address < Length;

        /// <inheritdoc />
        public int ReadBytes(ulong address, byte[] buffer, int index, int count) =>
            _accessor.ReadArray((long) address, buffer, index, count);

        /// <inheritdoc />
        public void Dispose() => _accessor?.Dispose();
    }
}
