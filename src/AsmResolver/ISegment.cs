namespace AsmResolver
{
    /// <summary>
    /// Represents a single chunk of data residing in a file or memory space.
    /// </summary>
    public interface ISegment
    {
        /// <summary>
        /// Gets the physical starting offset of the segment.
        /// </summary>
        uint FileOffset
        {
            get;
        }

        /// <summary>
        /// Gets the virtual address relative to the beginning of the section that the segment is located in. 
        /// </summary>
        uint Rva
        {
            get;
        }

        /// <summary>
        /// Assigns a new file and virtual offset to the segment and all its sub-components.
        /// </summary>
        /// <param name="newFileOffset">The new file offset.</param>
        /// <param name="newRva">The new virtual offset.</param>
        void UpdateOffsets(uint newFileOffset, uint newRva);

        /// <summary>
        /// Computes the number of bytes that the segment contains.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        uint GetPhysicalSize();

        /// <summary>
        /// Computes the number of bytes the segment will contain when it is mapped into memory.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        uint GetVirtualSize();

        /// <summary>
        /// Serializes the segment to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write the data to.</param>
        void Write(IBinaryStreamWriter writer);
        
    }

    public static partial class Extensions
    {
        public static uint Align(this uint value, uint alignment)
        {
            alignment--;
            return ((value + alignment) & ~alignment) - value;
        }
    }
}