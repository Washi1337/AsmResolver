using System;
using System.IO.MemoryMappedFiles;

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

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <inheritdoc />
        public unsafe int ReadBytes(ulong address, Span<byte> buffer)
        {
            var handle = _accessor.SafeMemoryMappedViewHandle;
            int actualLength = (int) Math.Min(handle.ByteLength, (uint) buffer.Length);
#if NET6_0_OR_GREATER
            handle.ReadSpan(address, buffer);
#else
            byte* pointer = null;

            try
            {
                handle.AcquirePointer(ref pointer);
                new ReadOnlySpan<byte>(pointer, actualLength).CopyTo(buffer);
            }
            finally
            {
                if (pointer != null)
                {
                    handle.ReleasePointer();
                }
            }
#endif
            return actualLength;
        }
#endif

        /// <inheritdoc />
        public void Dispose() => _accessor?.Dispose();
    }
}
