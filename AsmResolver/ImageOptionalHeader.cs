using System;
using System.Collections.Generic;

namespace AsmResolver
{
    /// <summary>
    /// Represents a 32-bit or 64-bit optional header in a windows assembly image.
    /// </summary>
    public class ImageOptionalHeader : FileSegment
    {

        internal static ImageOptionalHeader FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var header = new ImageOptionalHeader
            {
                StartOffset = reader.Position,
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
                    throw new NotSupportedException(string.Format("Unrecognized or unsupported executable format."));
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
            header.Subsystem = (ImageSubSystem)reader.ReadUInt16();
            header.DllCharacteristics = (ImageDllCharacteristics)reader.ReadUInt16();

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

            int dataDirectories = context.Parameters.ForceDataDirectoryCount
                ? context.Parameters.DataDirectoryCount
                : (int) header.NumberOfRvaAndSizes;

            for (int i = 0; i < dataDirectories; i++)
                header.DataDirectories.Add(ImageDataDirectory.FromReadingContext(context));

            return header;
        }
        
        public ImageOptionalHeader()
        {
            // TODO: set default values.
            DataDirectories = new List<ImageDataDirectory>();
        }
        
        /// <summary>
        /// Gets or sets the magic optional header signature, determining whether the image is a PE32 (32-bit) or a PE32+ (64-bit) assembly image.
        /// </summary>
        public OptionalHeaderMagic Magic
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major linker version used to link the windows assembly image.
        /// </summary>
        public byte MajorLinkerVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor linker version used to link the windows assembly image.
        /// </summary>
        public byte MinorLinkerVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total amount of bytes the code sections use.
        /// </summary>
        public uint SizeOfCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total amount of bytes the initialized data sections use.
        /// </summary>
        public uint SizeOfInitializedData
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the total amount of bytes the uninitialized data sections use.
        /// </summary>
        public uint SizeOfUninitializedData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address to the entrypoint of the windows assembly image.
        /// </summary>
        public uint AddressOfEntrypoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relative virtual address to the begin of the code section, when loaded into memory.
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
        }

        /// <summary>
        /// Gets or sets the alignment of the sections when loaded into memory. Must be greater or equal to <see cref="FileAlignment"/>. Default is the page size for the architecture.
        /// </summary>
        public uint SectionAlignment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the alignment of the raw data of sections in the image file. Must be a power of 2 between 512 and 64,000.
        /// </summary>
        public uint FileAlignment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum major version of the operating system required to run the windows assembly image.
        /// </summary>
        public ushort MajorOperatingSystemVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum minor version of the operating system required to run the windows assembly image.
        /// </summary>
        public ushort MinorOperatingSystemVersion
        {
            get;
            set;
        }

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
        }
        
        /// <summary>
        /// Gets or sets the minor version of the subsystem.
        /// </summary>
        public ushort MinorSubsystemVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Reserved, should be zero.
        /// </summary>
        public uint Win32VersionValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size in bytes of the windows assembly image, including all headers. Must be a multiple of <see cref="SectionAlignment"/>.
        /// </summary>
        public uint SizeOfImage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the headers of the windows assembly image, including the DOS-, PE- and section headers, rounded to <see cref="FileAlignment"/>.
        /// </summary>
        public uint SizeOfHeaders
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the checksum of the windows assembly image.
        /// </summary>
        public uint CheckSum
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the subsystem to use when running the windows assembly image.
        /// </summary>
        public ImageSubSystem Subsystem
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the dynamic linked library characteristics of the windows assembly image.
        /// </summary>
        public ImageDllCharacteristics DllCharacteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the stack to reserve.
        /// </summary>
        public ulong SizeOfStackReserve
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the stack to commit.
        /// </summary>
        public ulong SizeOfStackCommit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the heap to reserve.
        /// </summary>
        public ulong SizeOfHeapReserve
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the heap to commit.
        /// </summary>
        public ulong SizeOfHeapCommit
        {
            get;
            set;
        }

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
        }

        /// <summary>
        /// Gets the data directory headers defined in the optional header.
        /// </summary>
        public IList<ImageDataDirectory> DataDirectories
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return 0xE0; // TODO: make dynamic
            throw new NotImplementedException();
            return (uint)(sizeof (ushort) +
                          2 * sizeof (byte) +
                          5 * sizeof (uint) +

                          sizeof (ulong) + // 2x uint or 1x ulong

                          2 * sizeof (uint) +
                          6 * sizeof (ushort) +
                          4 * sizeof (uint) +
                          2 * sizeof (ushort) +

                          4 * (Magic == OptionalHeaderMagic.Pe32 ? sizeof (uint) : sizeof (ulong)) +

                          2 * sizeof (uint) +
                          DataDirectories.Count * DataDirectories[0].GetPhysicalLength());

        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            long start = writer.Position;

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
                    throw new NotSupportedException(string.Format("Unrecognized or unsupported executable format."));
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
            writer.WriteUInt16((ushort)Subsystem);
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
                directory.Write(context);

            writer.WriteBytes(new byte[start + context.Assembly.NtHeaders.FileHeader.SizeOfOptionalHeader - writer.Position]);
        }
    }
    
    /// <summary>
    /// Provides valid values for the optional header magic.
    /// </summary>
    public enum OptionalHeaderMagic : ushort
    {
        /// <summary>
        /// The windows assembly image is a 32-bit assembly.
        /// </summary>
        Pe32 = 0x010B,
        /// <summary>
        /// The windows assembly image is a 64-bit assembly.
        /// </summary>
        Pe32Plus = 0x020B,
    }
}
