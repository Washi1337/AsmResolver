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
}