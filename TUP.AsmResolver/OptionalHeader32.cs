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
    /// Represents a 32-bit optional header of a <see cref="TUP.AsmResolver.Win32Assembly"/>.
    /// </summary>
    public class OptionalHeader32 : IOptionalHeader, IHeader
    {
        
        internal OptionalHeader32()
        {
            
        }        

        /// <summary>
        /// Gets the 32 bit optional header by specifing a 32 bit assembly.
        /// </summary>
        /// <param name="assembly">The assembly to read the optional header</param>
        /// <returns></returns>
        public static OptionalHeader32 FromAssembly(Win32Assembly assembly)
        {
            OptionalHeader32 a = new OptionalHeader32();
            a.assembly = assembly;
            a.header = assembly._headerReader;
            return a;
        }

        internal Win32Assembly assembly;
        internal PeHeaderReader header;
        Offset entrypoint;

        /// <summary>
        /// Gets the linker version
        /// </summary>
        public Version LinkerVersion
        {
            get
            {
                return new Version(header.optionalHeader32.MajorLinkerVersion, header.optionalHeader32.MinorLinkerVersion);
            }
            set
            {
                header.optionalHeader32.MajorLinkerVersion = (byte)value.Major;
                header.optionalHeader32.MinorLinkerVersion = (byte)value.Minor;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][1]);
                assembly._peImage.Writer.Write(new byte[] { (byte)value.Major, (byte)value.Minor });
            }
        }

        /// <summary>
        /// Gets the minium operating system version the portable executable requires.
        /// </summary>
        public Version MinimumOSVersion
        {
            get
            {
                return new Version(header.optionalHeader32.MajorOperatingSystemVersion, header.optionalHeader32.MinorOperatingSystemVersion);
            }
            set
            {
                header.optionalHeader32.MajorOperatingSystemVersion = (byte)value.Major;
                header.optionalHeader32.MinorOperatingSystemVersion = (byte)value.Minor;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][12]);
                assembly._peImage.Writer.Write(new byte[] { (byte)value.Major, (byte)value.Minor });
            }
        }

        /// <summary>
        /// Gets the minium sub system version the portable executable requires.
        /// </summary>
        public Version MinimumSubSystemVersion
        {
            get
            {
                return new Version(header.optionalHeader32.MajorSubsystemVersion, header.optionalHeader32.MinorSubsystemVersion);
            }
            set
            {
                header.optionalHeader32.MajorSubsystemVersion = (byte)value.Major;
                header.optionalHeader32.MinorSubsystemVersion = (byte)value.Minor;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][16]);
                assembly._peImage.Writer.Write(new byte[] { (byte)value.Major, (byte)value.Minor });
            }
        }

        /// <summary>
        /// Gets or sets the entrypoint address of the portable executable file.
        /// </summary>
        public Offset Entrypoint
        {
            get
            {
                if (entrypoint == null)
                    entrypoint = Offset.FromRva(header.optionalHeader32.AddressOfEntryPoint, assembly);
                return entrypoint;
            }
            set
            {
                int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][6];
                header.optionalHeader32.AddressOfEntryPoint = value.Rva;
                entrypoint = value;
                assembly._peImage.SetOffset(targetoffset);
                assembly._peImage.Writer.Write(value.Rva);
            }
        }

        /// <summary>
        /// Gets the header size of the portable executable file.
        /// </summary>
        public uint HeaderSize
        {
            get
            {
                return header.optionalHeader32.SizeOfHeaders;
            }
            set
            {
                header.optionalHeader32.SizeOfHeaders = value;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][20]);
                assembly._peImage.Writer.Write(value);
            }
        }

        /// <summary>
        /// Gets the image base (base offset) of the portable executable file. 
        /// </summary>
        public ulong ImageBase
        {
            get
            {
                return header.optionalHeader32.ImageBase;
            }
            set
            {
                header.optionalHeader32.ImageBase = (uint)value;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][9]);
                assembly._peImage.Writer.Write((uint)value);
            }
        }

        /// <summary>
        /// Gets the base offset of the code section of the portable executable file.
        /// </summary>
        public uint BaseOfCode
        {
            get
            {
                return header.optionalHeader32.BaseOfCode;
            }
            set
            {
                header.optionalHeader32.BaseOfCode = value;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][7]);
                assembly._peImage.Writer.Write(value);
            }
        }

        /// <summary>
        /// Gets the size of the code section of the portable executable file.
        /// </summary>
        public uint SizeOfCode
        {
            get
            {
                return header.optionalHeader32.SizeOfCode;
            }
            set
            {
                header.optionalHeader32.SizeOfCode = value;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][19]);
                assembly._peImage.Writer.Write(value);
            }
        }

        /// <summary>
        /// Gets the base offset of the data section of the portable executable file.
        /// </summary>
        public uint BaseOfData
        {
            get
            {
                    return header.optionalHeader32.BaseOfData;

            }
            set
            {
                header.optionalHeader32.BaseOfData = value;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][8]);
                assembly._peImage.Writer.Write(value);
            }
        }

        /// <summary>
        /// Gets the file alignment of the portable executable file.
        /// </summary>
        public uint FileAlignment
        {
            get
            {
                return header.optionalHeader32.FileAlignment;
            }
            set
            {
                header.optionalHeader32.FileAlignment = value;
                assembly._peImage.SetOffset(RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][11]);
                assembly._peImage.Writer.Write(value);
            }
        }

        /// <summary>
        /// Gets or sets the sub system representation the portable executable file runs in.
        /// </summary>
        public SubSystem SubSystem
        {
            get
            {
                return (SubSystem)header.optionalHeader32.Subsystem;
            }
            set
            {
                byte sys = (byte)value;
                assembly._peImage.SetOffset(RawOffset +  Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][22]);
                assembly._peImage.Writer.Write(sys);
                header.optionalHeader32.Subsystem = sys;
            }
        }

        /// <summary>
        /// Returns true if the header file is 32-bit.
        /// </summary>
        public bool Is32Bit
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the Dynamic Loaded Library Flags of the portable executable file.
        /// </summary>
        public LibraryFlags LibraryFlags
        {
            get
            {
                return (LibraryFlags)header.optionalHeader32.DllCharacteristics;
            }
            set
            {
                int offset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][23];
                assembly._peImage.SetOffset(offset);
                assembly._peImage.Writer.Write((ushort)value);
                header.optionalHeader32.DllCharacteristics = (UInt16)value;
            }
        }

        /// <summary>
        /// Gets the raw file offset of the header.
        /// </summary>
        public long RawOffset
        {
            get
            {
                return header.optionalheaderoffset;
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

        public uint NumberOfRvaAndSizes
        {
            get
            {
                return header.optionalHeader32.NumberOfRvaAndSizes;
            }
            set
            {
                int offset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER32)][29];
                assembly._peImage.SetOffset(offset);
                assembly._peImage.Writer.Write(value);
                header.optionalHeader32.NumberOfRvaAndSizes = value;
            }
        }
        
        public DataDirectory[] DataDirectories
        {
            get { return header.datadirectories.ToArray(); }
        }
    }
}
