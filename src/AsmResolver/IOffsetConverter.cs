using System;

namespace AsmResolver
{
    /// <summary>
    /// Provides members for converting virtual addresses to file offsets and vice versa. 
    /// </summary>
    public interface IOffsetConverter
    {
        /// <summary>
        /// Converts a file offset to the virtual address when it is loaded into memory.
        /// </summary>
        /// <param name="fileOffset">The file offset to convert.</param>
        /// <returns>The virtual address, relative to the image base.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the file offset falls outside of the range of the convertible file offsets.</exception>
        uint FileOffsetToRva(uint fileOffset);
        
        /// <summary>
        /// Converts a virtual address to the physical file offset.
        /// </summary>
        /// <param name="rva">The virtual address, relative to the image base, to convert.</param>
        /// <returns>The file offset.</returns>
        /// /// <exception cref="ArgumentOutOfRangeException">Occurs when the virtual address falls outside of the range of the convertible addresses.</exception>
        uint RvaToFileOffset(uint rva);
    }
}