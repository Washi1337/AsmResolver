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
        /// <param name="offset">The number of bytes to skip after the beginning of the segment.</param>
        public RelativeReference(IOffsetProvider @base, int offset)
        {
            Base = @base;
            Offset = offset;
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
        public int Offset
        {
            get;
        }

        /// <inheritdoc />
        public uint FileOffset => (uint) (Base.FileOffset + Offset);

        /// <inheritdoc />
        public uint Rva => (uint) (Base.Rva + Offset);

        /// <inheritdoc />
        public bool CanUpdateOffsets => Base.CanUpdateOffsets;

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) =>
            Base.UpdateOffsets((uint) (newFileOffset - Offset), (uint) (newRva - Offset));

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
                reader.FileOffset = (uint) (reader.FileOffset + Offset);
                return reader;
            }
            
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public ISegment GetSegment() => throw new InvalidOperationException();
    }
    
}