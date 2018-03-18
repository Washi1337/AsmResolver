namespace AsmResolver
{
    /// <summary>
    /// Represents a generic file segment in a windows image.
    /// </summary>
    public abstract class FileSegment
    {
        /// <summary>
        /// Gets or sets the absoulte file offset the segment starts at.
        /// </summary>
        public long StartOffset
        {
            get;
            set;
        }

        public long GetRva(IOffsetConverter converter)
        {
            return converter.FileOffsetToRva(StartOffset);
        }

        /// <summary>
        /// Computes the physical length of the segment.
        /// </summary>
        /// <returns>The physical length of the segment in bytes.</returns>
        public abstract uint GetPhysicalLength();

        /// <summary>
        /// Writes the segment to a specific writing context.
        /// </summary>
        /// <param name="context">The context to use.</param>
        public abstract void Write(WritingContext context);

        /// <summary>
        /// Aligns a specific offset to a specific boundary.
        /// </summary>
        /// <param name="value">The value to align.</param>
        /// <param name="align">The block length of the alignment to use.</param>
        /// <returns>An aligned offset.</returns>
        public static uint Align(uint value, uint align)
        {
            align--;
            return (value + align) & ~align;
        }
    }
}
