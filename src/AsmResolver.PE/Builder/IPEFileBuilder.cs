using AsmResolver.PE.File;

namespace AsmResolver.PE.Builder
{
    /// <summary>
    /// Provides members for constructing a PE file from a PE image.
    /// </summary>
    public interface IPEFileBuilder
    {
        /// <summary>
        /// Assembles a new PE file based on a PE image.
        /// </summary>
        /// <param name="image">The image to assemble.</param>
        /// <returns>The assembled PE file.</returns>
        PEFile CreateFile(IPEImage image);
    }
}