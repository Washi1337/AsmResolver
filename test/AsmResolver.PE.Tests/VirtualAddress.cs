using System;

namespace AsmResolver.PE.Tests
{
    public readonly struct VirtualAddress : ISegmentReference
    {
        public VirtualAddress(uint rva)
        {
            Rva = rva;
        }
        
        ulong IOffsetProvider.Offset => Rva;

        public uint Rva
        {
            get;
        }

        bool IOffsetProvider.CanUpdateOffsets => false;

        void IOffsetProvider.UpdateOffsets(ulong newOffset, uint newRva) => throw new InvalidOperationException();

        public bool CanRead => false;

        bool ISegmentReference.IsBounded => false;

        IBinaryStreamReader ISegmentReference.CreateReader() => throw new InvalidOperationException();
        
        public ISegment GetSegment() => throw new InvalidOperationException();
    }
}