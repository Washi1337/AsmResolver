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

        internal Win32Assembly _assembly;
        internal PeHeaderReader _headerReader;

        /// <summary>
        /// Gets or sets the machine representation of the loaded portable executable.
        /// </summary>
        public Machine Machine
        {
            get
            {
                
                return (Machine)_headerReader.fileHeader.Machine;

            }
            set
            {
                int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_FILE_HEADER)][0];
                _assembly._peImage.SetOffset(targetoffset);
                _assembly.Image.Writer.Write((ushort)value);
                _headerReader.fileHeader.Machine = (ushort)value;
            }
        }

        /// <summary>
        /// Gets the amount of sections that is available in the PE.
        /// </summary>
        public ushort AmountOfSections
        {
            get
            {
                return _headerReader.fileHeader.NumberOfSections;
            }
            set
            {
                _headerReader.fileHeader.NumberOfSections = value;
                _assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_FILE_HEADER)][1]);
                _assembly.Image.Writer.Write(value);
            }
        }

        /// <summary>
        /// Gets the compiling date of the PE.
        /// </summary>
        public DateTime TimeStampDate
        {
            get
            {
                return _headerReader.TimeStamp;
            }
        }

        /// <summary>
        /// Gets the size of the Optional Header.
        /// </summary>
        public ushort OptionalHeaderSize
        {
            get
            {
                return _headerReader.fileHeader.SizeOfOptionalHeader;
            }
            set
            {
                _headerReader.fileHeader.SizeOfOptionalHeader = value;
                _assembly.Image.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_FILE_HEADER)][5]);
                _assembly.Image.Writer.Write(value);
            }
        }

        /// <summary>
        /// Gets or sets the flags of the loaded portable executable file.
        /// </summary>
        public ExecutableFlags ExecutableFlags
        {
            get
            {
                return (AsmResolver.ExecutableFlags)_headerReader.fileHeader.Characteristics;
            }
            set
            {
                int offset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_FILE_HEADER)][6];
                _assembly._peImage.SetOffset(offset);
                _assembly._peImage.Writer.Write((ushort)value);
                _headerReader.fileHeader.Characteristics = (UInt16)value;
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
            a._assembly = assembly;
            a._headerReader = assembly._headerReader;
            return a;
        }

        /// <summary>
        /// Gets the raw file offset of the header,
        /// </summary>
        public long RawOffset
        {
            get
            {
                return _headerReader.fileheaderoffset;
            }
        }

        /// <summary>
        /// Gets the parent assembly container of the MZ header.
        /// </summary>
        public Win32Assembly ParentAssembly
        {
            get
            {
                return _assembly;
            }
        }
    }
}
