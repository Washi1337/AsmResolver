using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents the optional header of a protable executable file.
    /// </summary>
    public interface IOptionalHeader : IHeader, IDataDirectoryProvider
    {
        /// <summary>
        /// Gets the minium operating system version the portable executable requires.
        /// </summary>
        Version MinimumOSVersion { get; }
        /// <summary>
        /// Gets the minium sub system version the portable executable requires.
        /// </summary>
        Version MinimumSubSystemVersion { get; }
        /// <summary>
        /// Gets the linker version
        /// </summary>
        Version LinkerVersion { get; }
        /// <summary>
        /// Gets or setsthe entrypoint address of the portable executable file.
        /// </summary>
        uint Entrypoint { get; set; }
        /// <summary>
        /// Gets the header size of the portable executable file.
        /// </summary>
        uint HeaderSize { get; }
        /// <summary>
        /// Gets the image base (base offset) of the portable executable file. 
        /// </summary>
        ulong ImageBase { get; }
        /// <summary>
        /// Gets the virtual base offset of the code section of the portable executable file.
        /// </summary>
        uint BaseOfCode { get; }
        /// <summary>
        /// Gets the size of the code section of the portable executable file.
        /// </summary>
        uint SizeOfCode { get; }
        /// <summary>
        /// Gets the virtual base offset of the data section of the portable executable file.
        /// </summary>
        uint BaseOfData { get; }
        /// <summary>
        /// Gets the file offset of the portable executable file.
        /// </summary>
        uint FileOffset { get; }

        /// <summary>
        /// Gets the sub system representation the portable executable file runs in.
        /// </summary>
        SubSystem SubSystem { get; set; }
        /// <summary>
        /// Gets the Dynamic Loaded Library Flags of the portable executable file.
        /// </summary>
        LibraryFlags LibraryFlags { get; set; }


        /// <summary>
        /// Indicates whether the optional header is 32 bit or not.
        /// </summary>
        bool Is32Bit { get; }

        uint NumberOfRvaAndSizes { get; set; }

        
    }
}
