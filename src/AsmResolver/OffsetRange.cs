using System;

namespace AsmResolver
{
    /// <summary>
    /// Represents an offset range determined by a start and end offset.
    /// </summary>
    public readonly struct OffsetRange
    {
        /// <summary>
        /// Converts a value tuple of unsigned integers to an offset range.
        /// </summary>
        /// <param name="tuple">The tuple to convert.</param>
        /// <returns>The constructed offset range.</returns>
        public static implicit operator OffsetRange((ulong Start, ulong End) tuple) => 
            new OffsetRange(tuple.Start, tuple.End);

        /// <summary>
        /// Creates a new offset range.
        /// </summary>
        /// <param name="start">The start offset.</param>
        /// <param name="end">The end offset, this offset is exclusive.</param>
        /// <exception cref="ArgumentException">Occurs when the provided start offset is bigger than the end offset.</exception>
        public OffsetRange(ulong start, ulong end)
        {
            if (start > end)
                throw new ArgumentException("Start offset must be smaller or equal to end offset.");
            
            Start = start;
            End = end;
        }
        
        /// <summary>
        /// Gets the start offset.
        /// </summary>
        public ulong Start
        {
            get;
        }

        /// <summary>
        /// Gets the end offset. This offset is exclusive.
        /// </summary>
        public ulong End
        {
            get;
        }

        /// <summary>
        /// Gets the length of the range.
        /// </summary>
        public int Length => (int) (End - Start);

        /// <summary>
        /// Gets a value indicating whether the range is empty.
        /// </summary>
        public bool IsEmpty => Start == End;

        /// <summary>
        /// Determines whether the provided offset falls within the range.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns><c>true</c> if the offset falls within the range, <c>false</c> otherwise.</returns>
        public bool Contains(ulong offset) => Start <= offset && End > offset;

        /// <summary>
        /// Determines whether the provided range is a subset of the range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns><c>true</c> if the range is a subset, <c>false</c> otherwise.</returns>
        public bool Contains(OffsetRange range) => Contains(range.Start) && Contains(range.End);

        /// <summary>
        /// Obtains the intersection between two ranges.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns>The intersection.</returns>
        public bool Intersects(OffsetRange other) => Contains(other.Start)
                                                     || Contains(other.End)
                                                     || other.Contains(Start)
                                                     || other.Contains(End);

        /// <summary>
        /// Determines whether the current range intersects with the provided range.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns><c>true</c> if the range intersects, <c>false</c> otherwise.</returns>
        public OffsetRange Intersect(OffsetRange other)
        {
            if (!Intersects(other))
                return new OffsetRange(0, 0);
            
            return new OffsetRange(
                Math.Max(Start, other.Start),
                Math.Min(End, other.End));
        }

        /// <summary>
        /// Determines the resulting ranges after excluding the provided range.
        /// </summary>
        /// <param name="other">The range to exclude.</param>
        /// <returns>The resulting ranges.</returns>
        public (OffsetRange left, OffsetRange right) Exclude(OffsetRange other)
        {
            var intersection = Intersect(other);
            if (intersection.IsEmpty)
                return (this, (End, End));
            if (Contains(other))
                return ((Start, other.Start), (other.End, End));
            if (Contains(other.Start))
                return (new OffsetRange(Start, other.Start), new OffsetRange(other.End, other.End));
            return ((other.Start, other.Start), (other.Start, End));
        }

        /// <summary>
        /// Deconstructs an offset range into its individual components.
        /// </summary>
        /// <param name="start">The start offset.</param>
        /// <param name="end">The exclusive end offset.</param>
        public void Deconstruct(out ulong start, out ulong end)
        {
            start = Start;
            end = End;
        }

        /// <inheritdoc />
        public override string ToString() => $"[{Start:X8}, {End:X8})";
    }
}