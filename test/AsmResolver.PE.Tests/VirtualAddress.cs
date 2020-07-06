using System;

namespace AsmResolver.PE.Tests
{
    public readonly struct VirtualAddress : ISegmentReference
    {
        public VirtualAddress(uint rva)
        {
            Rva = rva;
        }
        
        uint IOffsetProvider.FileOffset => Rva;

        public uint Rva
        {
            get;
        }

        bool IOffsetProvider.CanUpdateOffsets => false;

        void IOffsetProvider.UpdateOffsets(uint newFileOffset, uint newRva) => throw new InvalidOperationException();

        public bool CanRead => false;

        bool ISegmentReference.IsBounded => false;

        IBinaryStreamReader ISegmentReference.CreateReader() => throw new InvalidOperationException();
        
        public ISegment GetSegment() => throw new InvalidOperationException();
    }
}