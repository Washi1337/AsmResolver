using System;

namespace AsmResolver.PE.File
{
    public readonly struct PESegmentReference : ISegmentReference
    {
        private readonly PEFile _peFile;

        public PESegmentReference(PEFile peFile, uint rva)
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
        void IOffsetProvider.UpdateOffsets(ulong newFileOffset, uint newRva) => throw new InvalidOperationException();

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader() => _peFile.CreateReaderAtRva(Rva);

        /// <inheritdoc />
        public ISegment GetSegment() => throw new InvalidOperationException();
    }
}