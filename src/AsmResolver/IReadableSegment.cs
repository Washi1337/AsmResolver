using System;
using System.IO;

namespace AsmResolver
{
    /// <summary>
    /// Represents a segment with contents that is readable by a binary stream reader.
    /// </summary>
    public interface IReadableSegment : ISegment
    {
        /// <summary>
        /// Creates a new binary reader that reads the raw contents of the segment.
        /// </summary>
        /// <param name="fileOffset">The starting file offset of the reader.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <returns>The created binary reader.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when <paramref name="fileOffset"/> is not within the range of the segment.</exception>
        /// <exception cref="EndOfStreamException">Occurs when <paramref name="size"/> indicates a too large length.</exception>
        IBinaryStreamReader CreateReader(uint fileOffset, uint size);
        
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Creates a new binary reader that reads the raw contents of the segment.
        /// </summary>
        /// <param name="segment">The segment to read from.</param>
        /// <returns>The created binary reader.</returns>
        public static IBinaryStreamReader CreateReader(this IReadableSegment segment)
        {
            return segment.CreateReader(segment.FileOffset, segment.GetPhysicalSize());
        }

        /// <summary>
        /// Creates a new binary reader that reads the raw contents of the segment.
        /// </summary>
        /// <returns>The created binary reader.</returns>
        /// <param name="segment">The segment to read from.</param>
        /// <param name="fileOffset">The starting file offset of the reader.</param>
        public static IBinaryStreamReader CreateReader(this IReadableSegment segment, uint fileOffset)
        {
            return segment.CreateReader(fileOffset, segment.GetPhysicalSize() - (fileOffset - segment.FileOffset));
        }

        /// <summary>
        /// Reads the segment and puts the data in a byte array.
        /// </summary>
        /// <param name="segment">The segment to read.</param>
        /// <returns>The byte array that was read.</returns>
        public static byte[] ToArray(this IReadableSegment segment)
        {
            return segment.CreateReader().ReadToEnd();
        }
        
    }
}