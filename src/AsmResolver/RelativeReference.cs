using System;

namespace AsmResolver
{
    /// <summary>
    /// Represents a pointer or reference that is relative to the beginning of another segment or structure.
    /// </summary>
    public readonly struct RelativeReference : ISegmentReference
    {
        /// <summary>
        /// Creates a new relative reference.
        /// </summary>
        /// <param name="base">The segment the reference is relative to.</param>
        /// <param name="additive">The number of bytes to skip after the beginning of the segment.</param>
        public RelativeReference(IOffsetProvider @base, int additive)
        {
            Base = @base;
            Additive = additive;
        }
        
        /// <summary>
        /// Gets the segment or structure that this reference is relative to. 
        /// </summary>
        public IOffsetProvider Base
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes to skip after the beginning of the segment indicated by <see cref="Base"/>.
        /// </summary>
        public int Additive
        {
            get;
        }

        /// <inheritdoc />
        public ulong Offset => Base.Offset + (ulong) Additive;

        /// <inheritdoc />
        public uint Rva => (uint) (Base.Rva + Additive);

        /// <inheritdoc />
        public bool CanUpdateOffsets => Base.CanUpdateOffsets;

        /// <inheritdoc />
        public void UpdateOffsets(ulong newOffset, uint newRva) =>
            Base.UpdateOffsets( newOffset - (ulong) Additive, (uint) (newRva - Additive));

        /// <inheritdoc />
        public bool CanRead => Base is ISegmentReference reference && reference.CanRead;

        /// <inheritdoc />
        public bool IsBounded => false;

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader()
        {
            if (CanRead)
            {
                var reader = ((ISegmentReference) Base).CreateReader();
                reader.Offset = reader.Offset + (ulong) Additive;
                return reader;
            }
            
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public ISegment GetSegment() => throw new InvalidOperationException();
    }
    
}