namespace AsmResolver
{
    /// <summary>
    /// Represents a single chunk of data residing in a file or memory space.
    /// </summary>
    public interface ISegment
    {
        /// <summary>
        /// Gets or sets the starting offset of the segment.
        /// </summary>
        long StartOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Computes the number of bytes that the segment contains.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        int GetPhysicalLength();

        /// <summary>
        /// Serializes the segment to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write the data to.</param>
        void Write(IBinaryStreamWriter writer);
    }
    
}