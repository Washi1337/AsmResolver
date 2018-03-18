namespace AsmResolver
{
    /// <summary>
    /// Provides methods for converting offsets.
    /// </summary>
    public interface IOffsetConverter
    {
        /// <summary>
        /// Converts a relative virtual address (RVA) to its absolute file offset.
        /// </summary>
        /// <param name="rva">The relative virtual address (RVA) to convert.</param>
        /// <returns>The absolute file offset.</returns>
        long RvaToFileOffset(long rva);

        /// <summary>
        /// Converts an absolute file offset to its relative virtual address (RVA).
        /// </summary>
        /// <param name="fileOffset">The absolute file offset to convert.</param>
        /// <returns>The relative virtual address (RVA).</returns>
        long FileOffsetToRva(long fileOffset);
    }
}
