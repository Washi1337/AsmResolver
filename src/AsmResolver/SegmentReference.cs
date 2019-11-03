using System;

namespace AsmResolver
{
    /// <summary>
    /// Provides a base implementation for a reference to a segment in a binary file.
    /// </summary>
    public readonly struct SegmentReference : ISegmentReference
    {
        public SegmentReference(IReadableSegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));
        }
        
        /// <inheritdoc />
        public uint FileOffset => Segment.FileOffset;

        /// <inheritdoc />
        public uint Rva => Segment.Rva;
        
        /// <inheritdoc />
        public bool CanUpdateOffsets => Segment.CanUpdateOffsets;
        
        /// <inheritdoc />
        public bool IsBounded => true;

        /// <summary>
        /// Gets the referenced segment.
        /// </summary>
        public IReadableSegment Segment
        {
            get;
        }
        
        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) => Segment.UpdateOffsets(newFileOffset, newRva);

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader() => Segment.CreateReader();

    }
}