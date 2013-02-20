using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    
    /// <summary>
    /// Flags that describes a portable executable file.
    /// </summary>
    [Flags]
    public enum ExecutableFlags
    {
        /// <summary>
        /// Indicates that the file does not contain base relocations and must therefore be loaded at its preferred base address. 
        /// </summary>
        RelocationsStripped = 0x1,
        /// <summary>
        /// Indicates that the image file is valid and can be run. 
        /// </summary>
        ExecutableFile = 0x02,
        /// <summary>
        /// COFF line numbers have been removed.
        /// </summary>
        LineNumbersStripped = 0x4,
        /// <summary>
        /// COFF symbol table entries for local symbols have been removed. 
        /// </summary>
        LocalSymsStripped = 0x8,
        /// <summary>
        /// Obsolete. Aggressively trim working set. 
        /// </summary>
        AggressiveWSTrim = 0x10,
        /// <summary>
        /// Application can handle bigger than 2 GB addresses.
        /// </summary>
        LargeAddressAware = 0x20,
        /// <summary>
        /// This flag is reserved for future use.
        /// </summary>
        Machine16Bit = 0x40,
        /// <summary>
        /// Little endian: the least significant bit (LSB) precedes the most significant bit (MSB) in memory. 
        /// </summary>
        BytesReversedLO = 0x80,
        /// <summary>
        /// Machine is based on a 32-bit-word architecture.
        /// </summary>
        Machine32Bit = 0x100,
        /// <summary>
        /// Debugging information is removed from the image file.
        /// </summary>
        DebuggingInfoStripped = 0x200,
        /// <summary>
        /// If the image is on removable media, fully load it and copy it to the swap file.
        /// </summary>
        RemovableRunFromSwap = 0x400,
        /// <summary>
        /// If the image is on network media, fully load it and copy it to the swap file.
        /// </summary>
        NetRunFromSwap = 0x800,
        /// <summary>
        /// The image file is a system file, not a user program.
        /// </summary>
        System = 0x1000,
        /// <summary>
        /// The File is a DLL Library.
        /// </summary>
        DynamicLoadedLibraryFile = 0x2000,
        /// <summary>
        /// The file should be run only on a uniprocessor machine.
        /// </summary>
        UpSystemOnly = 0x4000,
        /// <summary>
        /// Big endian: the MSB precedes the LSB in memory. This flag is deprecated and should be zero.
        /// </summary>
        BytesReversedHI = 0x8000,
    }
}
