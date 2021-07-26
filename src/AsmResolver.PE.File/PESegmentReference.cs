using System;
using AsmResolver.IO;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Represents a reference to a segment of a PE file.
    /// </summary>
    public sealed class PESegmentReference : ISegmentReference
    {
        private readonly IPEFile _peFile;

        /// <summary>
        /// Creates a new PE reference.
        /// </summary>
        /// <param name="peFile">The underlying PE file.</param>
        /// <param name="rva">The virtual address of the segment.</param>
        internal PESegmentReference(IPEFile peFile, uint rva)
        {
            _peFile = peFile;
            Rva = rva;
        }

        /// <inheritdoc />
        public ulong Offset => _peFile.TryGetSectionContainingRva(Rva, out _)
            ? _peFile.RvaToFileOffset(Rva)
            : 0u;

        /// <inheritdoc />
        public uint Rva
        {
            get;
        }

        /// <inheritdoc />
        bool IOffsetProvider.CanUpdateOffsets => false;

        /// <inheritdoc />
        public bool CanRead => _peFile.TryGetSectionContainingRva(Rva, out _);

        /// <inheritdoc />
        public bool IsBounded => false;

        /// <inheritdoc />
        void IOffsetProvider.UpdateOffsets(ulong newOffset, uint newRva) => throw new InvalidOperationException();

        /// <inheritdoc />
        public BinaryStreamReader CreateReader() => _peFile.CreateReaderAtRva(Rva);

        /// <inheritdoc />
        public ISegment? GetSegment() => throw new InvalidOperationException();
    }
}
