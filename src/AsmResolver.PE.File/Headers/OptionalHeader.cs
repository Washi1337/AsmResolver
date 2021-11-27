using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents a 32-bit or 64-bit optional header in a portable executable (PE) file.
    /// </summary>
    public class OptionalHeader : SegmentBase
    {
        /// <summary>
        /// Indicates the offset of the SizeOfImage field in the optional header.
        /// </summary>
        public const uint OptionalHeaderSizeOfImageFieldOffset = 56;

        /// <summary>
        /// Indicates the static size of an optional header in a 32-bit portable executable file, excluding the
        /// data directory entries.
        /// </summary>
        public const uint OptionalHeader32SizeExcludingDataDirectories = 0x60;

        /// <summary>
        /// Indicates the static size of an optional header in a 64-bit portable executable file, excluding the
        /// data directory entries.
        /// </summary>
        public const uint OptionalHeader64SizeExcludingDataDirectories = 0x70;

        /// <summary>
        /// Indicates the default number of data directory entries in an optional header.
        /// </summary>
        public const int DefaultNumberOfRvasAndSizes = 16;

        /// <summary>
        /// Reads an optional header from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="ignoreNumberOfRvasAndSizes">Indicates whether the number of data directories should be ignored
        /// and <see cref="DefaultNumberOfRvasAndSizes"/> should be used instead.</param>
        /// <returns>The optional header.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the input stream is malformed.</exception>
        public static OptionalHeader FromReader(ref BinaryStreamReader reader, bool ignoreNumberOfRvasAndSizes = true)
        {
            var header = new OptionalHeader
            {
                Offset = reader.Offset,
                Rva = reader.Rva,
                Magic = (OptionalHeaderMagic)reader.ReadUInt16(),
                MajorLinkerVersion = reader.ReadByte(),
                MinorLinkerVersion = reader.ReadByte(),
                SizeOfCode = reader.ReadUInt32(),
                SizeOfInitializedData = reader.ReadUInt32(),
                SizeOfUninitializedData = reader.ReadUInt32(),
                AddressOfEntrypoint = reader.ReadUInt32(),
                BaseOfCode = reader.ReadUInt32()
            };

            switch (header.Magic)
            {
                case OptionalHeaderMagic.Pe32:
                    header.BaseOfData = reader.ReadUInt32();
                    header.ImageBase = reader.ReadUInt32();
                    break;
                case OptionalHeaderMagic.Pe32Plus:
                    header.ImageBase = reader.ReadUInt64();
                    break;
                default:
                    throw new BadImageFormatException("Unrecognized or unsupported optional header format.");
            }

            header.SectionAlignment = reader.ReadUInt32();
            header.FileAlignment = reader.ReadUInt32();
            header.MajorOperatingSystemVersion = reader.ReadUInt16();
            header.MinorOperatingSystemVersion = reader.ReadUInt16();
            header.MajorImageVersion = reader.ReadUInt16();
            header.MinorImageVersion = reader.ReadUInt16();
            header.MajorSubsystemVersion = reader.ReadUInt16();
            header.MinorSubsystemVersion = reader.ReadUInt16();
            header.Win32VersionValue = reader.ReadUInt32();
            header.SizeOfImage = reader.ReadUInt32();
            header.SizeOfHeaders = reader.ReadUInt32();
            header.CheckSum = reader.ReadUInt32();
            header.SubSystem = (SubSystem)reader.ReadUInt16();
            header.DllCharacteristics = (DllCharacteristics)reader.ReadUInt16();

            if (header.Magic == OptionalHeaderMagic.Pe32)
            {
                header.SizeOfStackReserve = reader.ReadUInt32();
                header.SizeOfStackCommit = reader.ReadUInt32();
                header.SizeOfHeapReserve = reader.ReadUInt32();
                header.SizeOfHeapCommit = reader.ReadUInt32();
            }
            else
            {
                header.SizeOfStackReserve = reader.ReadUInt64();
                header.SizeOfStackCommit = reader.ReadUInt64();
                header.SizeOfHeapReserve = reader.ReadUInt64();
                header.SizeOfHeapCommit = reader.ReadUInt64();
            }

            header.LoaderFlags = reader.ReadUInt32();
            header.NumberOfRvaAndSizes = reader.ReadUInt32();

            int dataDirectories = ignoreNumberOfRvasAndSizes
                ? DefaultNumberOfRvasAndSizes
                : (int) header.NumberOfRvaAndSizes;

            header.DataDirectories.Clear();
            for (int i = 0; i < dataDirectories; i++)
                header.DataDirectories.Add(DataDirectory.FromReader(ref reader));

            return header;
        }

        /// <summary>
        /// Gets or sets the magic optional header signature, determining whether the image is a PE32 (32-bit) or a PE32+ (64-bit) image.
        /// </summary>
        public OptionalHeaderMagic Magic
        {
            get;
            set;
        } = OptionalHeaderMagic.Pe32;

        /// <summary>
        /// Gets or sets the major linker version used to link the portable executable (PE) file.
        /// </summary>
        public byte MajorLinkerVersion
        {
            get;
            set;
        } = 0x30;

        /// <summary>
        /// Gets or sets the minor linker version used to link the portable executable (PE) file.
        /// </summary>
        public byte MinorLinkerVersion
        {
            get;
            set;
        } = 0x18;

        /// <summary>
        /// Gets or sets the total amount of bytes the code sections consist of.
        /// </summary>
        public uint SizeOfCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total amount of bytes the initialized data sections consist of.
        /// </summary>
        public uint SizeOfInitializedData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total amount of bytes the uninitialized data sections consist of.
        /// </summary>
        public uint SizeOfUninitializedData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address to the entrypoint of the portable executable (PE) file.
        /// </summary>
        public uint AddressOfEntrypoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address (RVA) to the beginning of the code section, when loaded into memory.
        /// </summary>
        public uint BaseOfCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address to the begin of the data section, when loaded into memory.
        /// </summary>
        public uint BaseOfData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the preferred address of the first byte of the image when loaded into memory. Must be a multiple of 64,000.
        /// </summary>
        public ulong ImageBase
        {
            get;
            set;
        } = 0x00400000;

        /// <summary>
        /// Gets or sets the alignment of the sections when loaded into memory. Must be greater or equal to
        /// <see cref="FileAlignment"/>. Default is the page size for the architecture.
        /// </summary>
        public uint SectionAlignment
        {
            get;
            set;
        } = 0x2000;

        /// <summary>
        /// Gets or sets the alignment of the raw data of sections in the image file. Must be a power of 2 between 512 and 64,000.
        /// </summary>
        public uint FileAlignment
        {
            get;
            set;
        } = 0x200;

        /// <summary>
        /// Gets or sets the minimum major version of the operating system required to run the portable executable (PE) file.
        /// </summary>
        public ushort MajorOperatingSystemVersion
        {
            get;
            set;
        } = 4;

        /// <summary>
        /// Gets or sets the minimum minor version of the operating system required to run the portable executable (PE) file.
        /// </summary>
        public ushort MinorOperatingSystemVersion
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// Gets or sets the major image version.
        /// </summary>
        public ushort MajorImageVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor image version.
        /// </summary>
        public ushort MinorImageVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version of the subsystem.
        /// </summary>
        public ushort MajorSubsystemVersion
        {
            get;
            set;
        } = 4;

        /// <summary>
        /// Gets or sets the minor version of the subsystem.
        /// </summary>
        public ushort MinorSubsystemVersion
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// Reserved, should be zero.
        /// </summary>
        public uint Win32VersionValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size in bytes of the portable executable (PE) file, including all headers. Must be a multiple of <see cref="SectionAlignment"/>.
        /// </summary>
        public uint SizeOfImage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the headers of the portable executable (PE) file, including the DOS-, PE- and section headers, rounded to <see cref="FileAlignment"/>.
        /// </summary>
        public uint SizeOfHeaders
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the checksum of the portable executable (PE) file.
        /// </summary>
        public uint CheckSum
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the subsystem to use when running the portable executable (PE) file.
        /// </summary>
        public SubSystem SubSystem
        {
            get;
            set;
        } = SubSystem.WindowsCui;

        /// <summary>
        /// Gets or sets the dynamic linked library characteristics of the portable executable (PE) file.
        /// </summary>
        public DllCharacteristics DllCharacteristics
        {
            get;
            set;
        } = DllCharacteristics.DynamicBase | DllCharacteristics.TerminalServerAware | DllCharacteristics.NxCompat;

        /// <summary>
        /// Gets or sets the size of the stack to reserve.
        /// </summary>
        public ulong SizeOfStackReserve
        {
            get;
            set;
        } = 0x001000000;

        /// <summary>
        /// Gets or sets the size of the stack to commit.
        /// </summary>
        public ulong SizeOfStackCommit
        {
            get;
            set;
        } = 0x00001000;

        /// <summary>
        /// Gets or sets the size of the heap to reserve.
        /// </summary>
        public ulong SizeOfHeapReserve
        {
            get;
            set;
        } = 0x001000000;

        /// <summary>
        /// Gets or sets the size of the heap to commit.
        /// </summary>
        public ulong SizeOfHeapCommit
        {
            get;
            set;
        } = 0x00001000;

        /// <summary>
        /// Obsolete.
        /// </summary>
        public uint LoaderFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of data directory headers defined in the optional header.
        /// </summary>
        public uint NumberOfRvaAndSizes
        {
            get;
            set;
        } = DefaultNumberOfRvasAndSizes;

        /// <summary>
        /// Gets the data directory headers defined in the optional header.
        /// </summary>
        public IList<DataDirectory> DataDirectories
        {
            get;
        } = new DataDirectory[DefaultNumberOfRvasAndSizes].ToList();

        /// <summary>
        /// Gets a data directory by its index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The data directory entry.</returns>
        public DataDirectory GetDataDirectory(DataDirectoryIndex index) => DataDirectories[(int) index];

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            // TODO: make configurable?
            return Magic == OptionalHeaderMagic.Pe32 ? 0xE0u : 0xF0u;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            ulong start = writer.Offset;

            writer.WriteUInt16((ushort)Magic);
            writer.WriteByte(MajorLinkerVersion);
            writer.WriteByte(MinorLinkerVersion);
            writer.WriteUInt32(SizeOfCode);
            writer.WriteUInt32(SizeOfInitializedData);
            writer.WriteUInt32(SizeOfUninitializedData);
            writer.WriteUInt32(AddressOfEntrypoint);
            writer.WriteUInt32(BaseOfCode);

            switch (Magic)
            {
                case OptionalHeaderMagic.Pe32:
                    writer.WriteUInt32(BaseOfData);
                    writer.WriteUInt32((uint)ImageBase);
                    break;
                case OptionalHeaderMagic.Pe32Plus:
                    writer.WriteUInt64(ImageBase);
                    break;
                default:
                    throw new BadImageFormatException("Unrecognized or unsupported optional header format.");
            }

            writer.WriteUInt32(SectionAlignment);
            writer.WriteUInt32(FileAlignment);
            writer.WriteUInt16(MajorOperatingSystemVersion);
            writer.WriteUInt16(MinorOperatingSystemVersion);
            writer.WriteUInt16(MajorImageVersion);
            writer.WriteUInt16(MinorImageVersion);
            writer.WriteUInt16(MajorSubsystemVersion);
            writer.WriteUInt16(MinorSubsystemVersion);
            writer.WriteUInt32(Win32VersionValue);
            writer.WriteUInt32(SizeOfImage);
            writer.WriteUInt32(SizeOfHeaders);
            writer.WriteUInt32(CheckSum);
            writer.WriteUInt16((ushort)SubSystem);
            writer.WriteUInt16((ushort)DllCharacteristics);

            if (Magic == OptionalHeaderMagic.Pe32)
            {
                writer.WriteUInt32((uint)SizeOfStackReserve);
                writer.WriteUInt32((uint)SizeOfStackCommit);
                writer.WriteUInt32((uint)SizeOfHeapReserve);
                writer.WriteUInt32((uint)SizeOfHeapCommit);
            }
            else
            {
                writer.WriteUInt64(SizeOfStackReserve);
                writer.WriteUInt64(SizeOfStackCommit);
                writer.WriteUInt64(SizeOfHeapReserve);
                writer.WriteUInt64(SizeOfHeapCommit);
            }

            writer.WriteUInt32(LoaderFlags);
            writer.WriteUInt32(NumberOfRvaAndSizes);

            foreach (var directory in DataDirectories)
                directory.Write(writer);

            writer.WriteZeroes((int) (GetPhysicalSize() - (writer.Offset - start)));
        }

    }
}
