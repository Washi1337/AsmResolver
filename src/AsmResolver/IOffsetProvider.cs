namespace AsmResolver
{
    /// <summary>
    /// Defines file offset and virtual address of a specific structure or segment in a binary file.
    /// </summary>
    public interface IOffsetProvider
    {
        /// <summary>
        /// Gets the physical starting offset of the segment.
        /// </summary>
        ulong Offset
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
        /// Determines whether this structure can be relocated to another offset or virtual address.
        /// </summary>
        bool CanUpdateOffsets
        {
            get;
        }

        /// <summary>
        /// Assigns a new file and virtual offset to the segment and all its sub-components.
        /// </summary>
        /// <param name="offseteOffset">The new file offset.</param>
        /// <param name="newRva">The new virtual offset.</param>
        void UpdateOffsets(ulong newOffset, uint newRva);
    }
}