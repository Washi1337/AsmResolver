using System;
using AsmResolver.IO;

namespace AsmResolver.PE.Tests
{
    public sealed class VirtualAddress : ISegmentReference
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

        public bool CanRead => false;

        bool ISegmentReference.IsBounded => false;

        BinaryStreamReader ISegmentReference.CreateReader() => throw new InvalidOperationException();

        public ISegment? GetSegment() => throw new InvalidOperationException();
    }
}
