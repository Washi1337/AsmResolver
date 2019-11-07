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
        public uint FileOffset => _peFile.RvaToFileOffset(Rva);

        /// <inheritdoc />
        public uint Rva
        {
            get;
        }

        /// <inheritdoc />
        bool IOffsetProvider.CanUpdateOffsets => false;

        /// <inheritdoc />
        public bool CanRead => true;

        /// <inheritdoc />
        public bool IsBounded => false;

        /// <inheritdoc />
        void IOffsetProvider.UpdateOffsets(uint newFileOffset, uint newRva) => throw new InvalidOperationException();

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader() => _peFile.CreateReaderAtRva(Rva);

        /// <inheritdoc />
        public ISegment GetSegment() => throw new InvalidOperationException();
    }
}