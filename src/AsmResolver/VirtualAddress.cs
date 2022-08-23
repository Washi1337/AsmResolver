using System;
using AsmResolver.IO;

namespace AsmResolver
{
    /// <summary>
    /// Represents a (relative) virtual address in a file.
    /// </summary>
    public sealed class VirtualAddress : ISegmentReference
    {
        /// <summary>
        /// Wraps a relative virtual address into a <see cref="ISegmentReference"/> object.
        /// </summary>
        /// <param name="rva"></param>
        public VirtualAddress(uint rva)
        {
            Rva = rva;
        }

        ulong IOffsetProvider.Offset => Rva;

        /// <inheritdoc />
        public uint Rva
        {
            get;
        }

        /// <inheritdoc />
        public bool CanRead => false;

        bool ISegmentReference.IsBounded => false;

        BinaryStreamReader ISegmentReference.CreateReader() => throw new InvalidOperationException();

        ISegment? ISegmentReference.GetSegment() => throw new InvalidOperationException();
    }
}
