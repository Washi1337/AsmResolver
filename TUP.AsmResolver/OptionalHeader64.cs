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
    /// Represents a 64-bit optional header of a <see cref="TUP.AsmResolver.Win32Assembly"/>.
    /// </summary>
    public class OptionalHeader64 : IOptionalHeader, IHeader
    {

        internal OptionalHeader64()
        {
        }

        /// <summary>
        /// Gets the 64 bit optional header by specifing a 64 bit assembly.
        /// </summary>
        /// <param name="assembly">The assembly to read the optional header</param>
        /// <returns></returns>
        public static OptionalHeader64 FromAssembly(Win32Assembly assembly)
        {
            OptionalHeader64 a = new OptionalHeader64();
            a.assembly = assembly;
            a.header = assembly.headerReader;
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
                return new Version(header.optionalHeader64.MajorLinkerVersion, header.optionalHeader64.MinorLinkerVersion);
            }
            set
            {
                header.optionalHeader32.MajorLinkerVersion = (byte)value.Major;
                header.optionalHeader32.MinorLinkerVersion = (byte)value.Minor;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][1], new byte[] { (byte)value.Major, (byte)value.Minor });
            }
        }

        /// <summary>
        /// Gets the minium operating system version the portable executable requires.
        /// </summary>
        public Version MinimumOSVersion
        {
            get
            {
                    return new Version(header.optionalHeader64.MajorOperatingSystemVersion, header.optionalHeader64.MinorOperatingSystemVersion);
            }
            set
            {
                header.optionalHeader32.MajorOperatingSystemVersion = (byte)value.Major;
                header.optionalHeader32.MinorOperatingSystemVersion = (byte)value.Minor;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][12], new byte[] { (byte)value.Major, (byte)value.Minor });
            }
        }

        /// <summary>
        /// Gets the minium sub system version the portable executable requires.
        /// </summary>
        public Version MinimumSubSystemVersion
        {
            get
            {
                    return new Version(header.optionalHeader64.MajorSubsystemVersion, header.optionalHeader64.MinorSubsystemVersion);

            }
            set
            {
                header.optionalHeader32.MajorSubsystemVersion = (byte)value.Major;
                header.optionalHeader32.MinorSubsystemVersion = (byte)value.Minor;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][16], new byte[] { (byte)value.Major, (byte)value.Minor });
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
                int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][6];
                header.optionalHeader64.AddressOfEntryPoint = value.Rva;
                entrypoint = value;
                assembly.peImage.Write(targetoffset, value.Rva);
            }
        }

        /// <summary>
        /// Gets the header size of the portable executable file.
        /// </summary>
        public uint HeaderSize
        {
            get
            {
                    return header.optionalHeader64.SizeOfHeaders;

            }
            set
            {
                header.optionalHeader32.SizeOfHeaders = value;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][20], value);
            }
        }

        /// <summary>
        /// Gets the image base (base offset) of the portable executable file. 
        /// </summary>
        public ulong ImageBase
        {
            get
            {
                    return header.optionalHeader64.ImageBase;

            }
            set
            {
                header.optionalHeader32.ImageBase = (uint)value;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][9], (uint)value);
            }
        }

        /// <summary>
        /// Gets the base offset of the code section of the portable executable file.
        /// </summary>
        public uint BaseOfCode
        {
            get
            {
                    return header.optionalHeader64.BaseOfCode;

            }
            set
            {
                header.optionalHeader32.BaseOfCode = value;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][7], value);
            }
        }

        /// <summary>
        /// Gets the size of the code section of the portable executable file.
        /// </summary>
        public uint SizeOfCode
        {
            get
            {
                return header.optionalHeader64.SizeOfCode;
            }
            set
            {
                header.optionalHeader32.SizeOfCode = value;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][19], value);
            }
        }

        /// <summary>
        /// Gets the base offset of the data section of the portable executable file.
        /// </summary>
        public uint BaseOfData
        {
            get
            {
                    return 0;

            }
            set
            {
                header.optionalHeader32.BaseOfData = value;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][8], value);
            }
        }

        /// <summary>
        /// Gets the file alignment of the portable executable file.
        /// </summary>
        public uint FileAlignment
        {
            get
            {
                    return header.optionalHeader64.FileAlignment;

            }
            set
            {
                header.optionalHeader32.FileAlignment = value;
                assembly.peImage.Write((int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][11], value);
            }

        }

        /// <summary>
        /// Gets the sub system representation the portable executable file runs in.
        /// </summary>
        public SubSystem SubSystem
        {
            get
            {
                    return (SubSystem)header.optionalHeader64.Subsystem;
                
            }
            set
            {
                byte sys = (byte)value;
                int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][22];

                assembly.peImage.Write(targetoffset, sys);
                header.optionalHeader32.Subsystem = sys;
            }
        }

        /// <summary>
        /// Returns true if the header file is 32-bit.
        /// </summary>
        public bool Is32Bit
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the Dynamic Loaded Library Flags of the portable executable file.
        /// </summary>
        public LibraryFlags LibraryFlags
        {
            get
            {
                
                    return (LibraryFlags)header.optionalHeader64.DllCharacteristics;
                

            }
            set
            {
                int offset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][23];
                assembly.peImage.Write(offset, (ushort)value);
                header.optionalHeader64.DllCharacteristics = (UInt16)value;
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
                return header.optionalHeader64.NumberOfRvaAndSizes;
            }
            set
            {
                int offset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_OPTIONAL_HEADER64)][28];
                assembly.peImage.Write(offset, value);
                header.optionalHeader64.NumberOfRvaAndSizes = value;
            }
        }

        public DataDirectory[] DataDirectories
        {
            get { return header.datadirectories.ToArray(); }
        }
    }
}
