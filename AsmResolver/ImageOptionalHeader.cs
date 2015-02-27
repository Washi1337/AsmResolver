using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
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

            for (int i = 0; i < header.NumberOfRvaAndSizes; i++)
                header.DataDirectories.Add(ImageDataDirectory.FromReadingContext(context));

            return header;
        }
        
        public ImageOptionalHeader()
        {
            // TODO: set default values.
            DataDirectories = new List<ImageDataDirectory>();
        }
        
        public OptionalHeaderMagic Magic
        {
            get;
            set;
        }

        public byte MajorLinkerVersion
        {
            get;
            set;
        }

        public byte MinorLinkerVersion
        {
            get;
            set;
        }

        public uint SizeOfCode
        {
            get;
            set;
        }

        public uint SizeOfInitializedData
        {
            get;
            set;
        }

        public uint SizeOfUninitializedData
        {
            get;
            set;
        }

        public uint AddressOfEntrypoint
        {
            get;
            set;
        }

        public uint BaseOfCode
        {
            get;
            set;
        }

        public uint BaseOfData
        {
            get;
            set;
        }

        public ulong ImageBase
        {
            get;
            set;
        }

        public uint SectionAlignment
        {
            get;
            set;
        }

        public uint FileAlignment
        {
            get;
            set;
        }

        public ushort MajorOperatingSystemVersion
        {
            get;
            set;
        }

        public ushort MinorOperatingSystemVersion
        {
            get;
            set;
        }

        public ushort MajorImageVersion
        {
            get;
            set;
        }

        public ushort MinorImageVersion
        {
            get;
            set;
        }

        public ushort MajorSubsystemVersion
        {
            get;
            set;
        }

        public ushort MinorSubsystemVersion
        {
            get;
            set;
        }

        public uint Win32VersionValue
        {
            get;
            set;
        }

        public uint SizeOfImage
        {
            get;
            set;
        }

        public uint SizeOfHeaders
        {
            get;
            set;
        }

        public uint CheckSum
        {
            get;
            set;
        }

        public ImageSubSystem Subsystem
        {
            get;
            set;
        }

        public ImageDllCharacteristics DllCharacteristics
        {
            get;
            set;
        }

        public ulong SizeOfStackReserve
        {
            get;
            set;
        }

        public ulong SizeOfStackCommit
        {
            get;
            set;
        }

        public ulong SizeOfHeapReserve
        {
            get;
            set;
        }

        public ulong SizeOfHeapCommit
        {
            get;
            set;
        }

        public uint LoaderFlags
        {
            get;
            set;
        }

        public uint NumberOfRvaAndSizes
        {
            get;
            set;
        }

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

    public enum OptionalHeaderMagic : ushort
    {
        Pe32 = 0x010B,
        Pe32Plus = 0x020B,
    }
}
