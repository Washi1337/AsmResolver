using System;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Provides members describing the operating system for which the file is designed.
    /// </summary>
    [Flags]
    public enum FileOS
    {
        /// <summary>
        /// Indicates the operating system for which the file was designed is unknown to the system. 
        /// </summary>
        Unknown = 0x00000000,
        
        /// <summary>
        /// Indicates the file was designed for 16-bit Windows. 
        /// </summary>
        Windows16 = 0x00000001,
        
        /// <summary>
        /// Indicates the file was designed for 16-bit Presentation Manager. 
        /// </summary>
        PresentationManager16 = 0x00000002,
        
        /// <summary>
        /// Indicates the file was designed for 32-bit Presentation Manager. 
        /// </summary>
        PresentationManager32 = 0x00000003,
        
        /// <summary>
        /// Indicates the file was designed for 32-bit Windows. 
        /// </summary>
        Windows32 = 0x00000004,
        
        /// <summary>
        /// Indicates the file was designed for MS-DOS. 
        /// </summary>
        Dos = 0x00010000,
        
        /// <summary>
        /// Indicates the file was designed for 16-bit OS/2. 
        /// </summary>
        OS216 = 0x00020000,
        
        /// <summary>
        /// Indicates the file was designed for 16-bit OS/2. 
        /// </summary>
        OS232 = 0x00030000,
        
        /// <summary>
        /// Indicates the file was designed for Windows NT. 
        /// </summary>
        NT = 0x00040000
    }
}