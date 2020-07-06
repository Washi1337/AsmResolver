using System;

namespace AsmResolver
{
    /// <summary>
    /// Provides a base implementation for a reference to a segment in a binary file.
    /// </summary>
    public readonly struct SegmentReference : ISegmentReference
    {
        /// <summary>
        /// Represents the null reference. 
        /// </summary>
        public static SegmentReference Null
        {
            get;
        } = new SegmentReference(null);
        
        public SegmentReference(ISegment segment)
        {
            Segment = segment;
        }
        
        /// <inheritdoc />
        public uint FileOffset => Segment?.FileOffset ?? 0;

        /// <inheritdoc />
        public uint Rva => Segment?.Rva ?? 0;
        
        /// <inheritdoc />
        public bool CanUpdateOffsets => Segment.CanUpdateOffsets;
        
        /// <inheritdoc />
        public bool IsBounded => true;
        
        /// <inheritdoc />
        public bool CanRead => Segment is IReadableSegment;

        /// <summary>
        /// Gets the referenced segment.
        /// </summary>
        public ISegment Segment
        {
            get;
        }
        
        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva) => Segment.UpdateOffsets(newFileOffset, newRva);

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader()
        {
            return CanRead
                ? ((IReadableSegment) Segment).CreateReader()
                : throw new InvalidOperationException("Cannot read the segment using a binary reader.");
        }

        ISegment ISegmentReference.GetSegment() => Segment;
    }
}