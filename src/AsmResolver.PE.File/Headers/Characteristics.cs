using System;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Provides valid attributes for describing a portable executable file.
    /// </summary>
    [Flags]
    public enum Characteristics : ushort
    {
        /// <summary>
        /// Indicates that the file does not contain base relocations and must therefore be loaded at
        /// its preferred base address.
        /// </summary>
        RelocsStripped = 0x0001,
        
        /// <summary>
        /// Indicates that the image file is valid and can be run. If this flag is not set, it indicates a linker error. 
        /// </summary>
        Image = 0x0002,
        
        /// <summary>
        /// Indicates COFF line numbers have been removed. This flag is deprecated and should be zero. 
        /// </summary>
        LineNumsStripped = 0x0004,
        
        /// <summary>
        /// Indicates COFF symbol table entries for local symbols have been removed. This flag is deprecated and
        /// should be zero. 
        /// </summary>
        LocalSymsStripped = 0x0008,
        
        /// <summary>
        /// Indicates an aggressively trim working set is used. This flag is deprecated for Windows 2000 and later
        /// and must be zero. 
        /// </summary>
        AggressiveWsTrim = 0x0010,
        
        /// <summary>
        /// Indicates the application can handle larger than 2GB addresses. 
        /// </summary>
        LargeAddressAware = 0x0020,
        
        /// <summary>
        /// Indicates the file uses little endian. This flag is deprecated and should be zero. 
        /// </summary>
        BytesReversedLo = 0x0080,
        
        /// <summary>
        /// Indicates the target machine is based on a 32-bit-word architecture. 
        /// </summary>
        Machine32Bit = 0x0100,
        
        /// <summary>
        /// Indicates debugging information is removed from the image file. 
        /// </summary>
        DebugStripped = 0x0200,
        
        /// <summary>
        /// Indicates the image should be fully loaded and copied to the swap file if the image is on removable media. 
        /// </summary>
        RemovableRunFromSwap = 0x0400,
        
        /// <summary>
        /// Indicates the image should be fully loaded and copied to the swap file if the image is on a network media.
        /// </summary>
        NetRunFromSwap = 0x0800,
        
        /// <summary>
        /// Indicates the image file is a system file, not a user program. 
        /// </summary>
        System = 0x1000,
        
        /// <summary>
        /// Indicates the image file is a dynamic-link library (DLL). Such files are considered executable files
        /// for almost all purposes, although they cannot be directly run. 
        /// </summary>
        Dll = 0x2000,
        
        /// <summary>
        /// Indicates the file should be run only on a uniprocessor machine. 
        /// </summary>
        UpSystemOnly = 0x4000,
        
        /// <summary>
        /// Indicates the file uses big endian. This flag is deprecated and should be zero. 
        /// </summary>
        BytesReversedHi = 0x8000,
    }
}