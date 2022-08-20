using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.IO;

namespace AsmResolver
{
    /// <summary>
    /// Provides a base implementation for a reference to a segment in a binary file.
    /// </summary>
    public sealed class SegmentReference : ISegmentReference
    {
        /// <summary>
        /// Represents the null reference.
        /// </summary>
        public static ISegmentReference Null
        {
            get;
        } = new SegmentReference(null);

        /// <summary>
        /// Creates a new reference to the provided segment.
        /// </summary>
        /// <param name="segment">The segment to reference.</param>
        public SegmentReference(ISegment? segment)
        {
            Segment = segment;
        }

        /// <inheritdoc />
        public ulong Offset => Segment?.Offset ?? 0;

        /// <inheritdoc />
        public uint Rva => Segment?.Rva ?? 0;

        /// <inheritdoc />
        public bool CanUpdateOffsets => Segment?.CanUpdateOffsets ?? false;

        /// <inheritdoc />
        public bool IsBounded => true;

        /// <inheritdoc />
        [MemberNotNullWhen(true, nameof(Segment))]
        public bool CanRead => Segment is IReadableSegment;

        /// <summary>
        /// Gets the referenced segment.
        /// </summary>
        public ISegment? Segment
        {
            get;
        }

        /// <inheritdoc />
        public BinaryStreamReader CreateReader()
        {
            return CanRead
                ? ((IReadableSegment) Segment).CreateReader()
                : throw new InvalidOperationException("Cannot read the segment using a binary reader.");
        }

        ISegment? ISegmentReference.GetSegment() => Segment;
    }
}
