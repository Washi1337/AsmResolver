using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents the file header of a loaded <see cref="TUP.AsmResolver.Win32Assembly"/>. This header contains common information about the Portable Executable.
    /// </summary>
    public class FileHeader : IHeader
    {

        internal Win32Assembly assembly;
        internal PeHeaderReader header;

        /// <summary>
        /// Gets or sets the machine representation of the loaded portable executable.
        /// </summary>
        public Machine Machine
        {
            get
            {
                
                return (Machine)header.fileHeader.Machine;

            }
            set
            {
                int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_FILE_HEADER)][0];
                assembly.Image.Write(targetoffset, (ushort)value);
                header.fileHeader.Machine = (UInt16)value;
            }
        }

        /// <summary>
        /// Gets the amount of sections that is available in the PE.
        /// </summary>
        public ushort AmountOfSections
        {
            get
            {
                return header.fileHeader.NumberOfSections;
            }
            set
            {
                header.fileHeader.NumberOfSections = value;
                assembly.Image.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_FILE_HEADER)][1], value);
            }
        }

        /// <summary>
        /// Gets the compiling date of the PE.
        /// </summary>
        public DateTime TimeStampDate
        {
            get
            {
                return header.TimeStamp;
            }
        }

        /// <summary>
        /// Gets the size of the Optional Header.
        /// </summary>
        public ushort OptionalHeaderSize
        {
            get
            {
                return header.fileHeader.SizeOfOptionalHeader;
            }
            set
            {
                header.fileHeader.SizeOfOptionalHeader = value;
                assembly.Image.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_FILE_HEADER)][5], value);
            }
        }

        /// <summary>
        /// Gets or sets the flags of the loaded portable executable file.
        /// </summary>
        public ExecutableFlags ExecutableFlags
        {
            get
            {
                return (AsmResolver.ExecutableFlags)header.fileHeader.Characteristics;
            }
            set
            {
                int offset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_FILE_HEADER)][6];
                assembly.peImage.Write(offset, (ushort)value);
                header.fileHeader.Characteristics = (UInt16)value;
            }
        }


        internal FileHeader()
        {
        }

        /// <summary>
        /// Gets the Portable Executeable's file header by specifing the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to read the mz header</param>
        /// <returns></returns>
        public static FileHeader FromAssembly(Win32Assembly assembly)
        {
            FileHeader a = new FileHeader();
            a.assembly = assembly;
            a.header = assembly.headerReader;
            return a;
        }

        /// <summary>
        /// Gets the raw file offset of the header,
        /// </summary>
        public long RawOffset
        {
            get
            {
                return header.fileheaderoffset;
            }
        }

        /// <summary>
        /// Gets the parent assembly container of the MZ header.
        /// </summary>
        public Win32Assembly ParentAssembly
        {
            get
            {
                return assembly;
            }
        }
    }
}
