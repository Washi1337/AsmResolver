namespace AsmResolver
{
    /// <summary>
    /// Defines file offset and virtual address of a specific structure or segment in a binary file.
    /// </summary>
    public interface IFileOffsetProvider
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
    }
}