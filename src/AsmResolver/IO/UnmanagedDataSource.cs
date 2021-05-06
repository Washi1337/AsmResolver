using System;
using System.Runtime.InteropServices;

namespace AsmResolver.IO
{
    /// <summary>
    /// Represents a data source that obtains its data from a block of unmanaged memory.
    /// </summary>
    public sealed unsafe class UnmanagedDataSource : IDataSource
    {
        private readonly void* _basePointer;

        public UnmanagedDataSource(void* basePointer, ulong length)
        {
            _basePointer = basePointer;
            Length = length;
        }

        /// <inheritdoc />
        public ulong BaseAddress => (ulong) _basePointer;

        /// <inheritdoc />
        public byte this[ulong address]
        {
            get
            {
                if (!IsValidAddress(address))
                    throw new ArgumentOutOfRangeException(nameof(address));
                return *(byte*) address;
            }
        }

        /// <inheritdoc />
        public ulong Length
        {
            get;
        }

        /// <inheritdoc />
        public bool IsValidAddress(ulong address) => address >= (ulong) _basePointer
                                                     && address - (ulong) _basePointer < Length;

        /// <inheritdoc />
        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            if (!IsValidAddress(address))
                return 0;

            ulong relativeIndex = address - (ulong) _basePointer;
            int actualLength = (int) Math.Min((ulong) count, Length - relativeIndex);
            Marshal.Copy((IntPtr) address, buffer, index, actualLength);
            return actualLength;
        }
    }
}
