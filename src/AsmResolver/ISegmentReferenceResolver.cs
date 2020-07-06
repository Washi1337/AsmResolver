namespace AsmResolver
{
    /// <summary>
    /// Provides members for resolving virtual addresses to a segment in a binary file.
    /// </summary>
    public interface ISegmentReferenceResolver
    {
        /// <summary>
        /// Resolves the provided virtual address to a segment reference.  
        /// </summary>
        /// <param name="rva">The virtual address of the segment.</param>
        /// <returns>The reference to the segment.</returns>
        ISegmentReference GetReferenceToRva(uint rva);
    }
}