namespace AsmResolver
{
    /// <summary>
    /// Provides members for creating references to a segment in a binary file.
    /// </summary>
    public interface ISegmentReferenceFactory
    {
        /// <summary>
        /// Resolves the provided virtual address to a segment reference.  
        /// </summary>
        /// <param name="rva">The virtual address of the segment.</param>
        /// <returns>The reference to the segment.</returns>
        ISegmentReference CreateReferenceToRva(uint rva);
    }
}