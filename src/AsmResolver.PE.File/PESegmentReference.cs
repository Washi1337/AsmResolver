using System;
using System.IO;
using AsmResolver.IO;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Represents a reference to a segment of a PE file.
    /// </summary>
    public sealed class PESegmentReference : ISegmentReference
    {
        private readonly PEFile _peFile;

        /// <summary>
        /// Creates a new PE reference.
        /// </summary>
        /// <param name="peFile">The underlying PE file.</param>
        /// <param name="rva">The virtual address of the segment.</param>
        internal PESegmentReference(PEFile peFile, uint rva)
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
        public bool CanRead
            => _peFile.TryGetSectionContainingRva(Rva, out var section)
            && section.IsReadable
            && section.ContainsFileOffset(Offset);

        /// <inheritdoc />
        public bool IsBounded => false;

        /// <summary>
        /// Gets a value indicating whether the reference points to a valid section within the PE file.
        /// </summary>
        public bool IsValidAddress => _peFile.TryGetSectionContainingRva(Rva, out _);

        /// <summary>
        /// Gets a value indicating whether the reference points to a valid physical section within the PE file.
        /// </summary>
        public bool PointsToPhysicalData
            => _peFile.TryGetSectionContainingRva(Rva, out var section)
            && section.ContainsFileOffset(Offset);

        /// <inheritdoc />
        public BinaryStreamReader CreateReader() => _peFile.CreateReaderAtRva(Rva);

        /// <inheritdoc />
        public ISegment? GetSegment() => throw new InvalidOperationException();

        /// <inheritdoc />
        public override string ToString() => string.IsNullOrEmpty(_peFile.FilePath)
            ? $"[module]+0x{Rva:X8}"
            : $"{Path.GetFileName(_peFile.FilePath)}+0x{Rva:X8}";
    }
}
