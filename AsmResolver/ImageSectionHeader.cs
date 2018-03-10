using System;
using System.Text;

namespace AsmResolver
{
    public class ImageSectionHeader : FileSegment, IOffsetConverter
    {
        private string _name;

        internal static ImageSectionHeader FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var header = new ImageSectionHeader
            {
                StartOffset = reader.Position,
                Name = Encoding.ASCII.GetString(reader.ReadBytes(8)),
                VirtualSize = reader.ReadUInt32(),
                VirtualAddress = reader.ReadUInt32(),
                SizeOfRawData = reader.ReadUInt32(),
                PointerToRawData = reader.ReadUInt32(),
                PointerToRelocations = reader.ReadUInt32(),
                PointerToLinenumbers = reader.ReadUInt32(),
                NumberOfRelocations = reader.ReadUInt16(),
                NumberOfLinenumbers = reader.ReadUInt16(),
                Attributes = (ImageSectionAttributes)reader.ReadUInt32(),
                
            };

            var sectionReader = context.Reader.CreateSubReader(
                header.PointerToRawData, 
                (int) header.SizeOfRawData);

            header.Section = new ImageSection(header, sectionReader);

            return header;
        }

        public ImageSectionHeader()
        {
            Section = new ImageSection(this);
        }

        public WindowsAssembly Assembly
        {
            get;
            internal set;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value.Length > 8)
                    throw new NotSupportedException("Name of section cannot be longer than 8 characters.");
                _name = value;
            }
        }

        public uint VirtualSize
        {
            get;
            set;
        }

        public uint VirtualAddress
        {
            get;
            set;
        }

        public uint SizeOfRawData
        {
            get;
            set;
        }

        public uint PointerToRawData
        {
            get;
            set;
        }

        public uint PointerToRelocations
        {
            get;
            set;
        }

        public uint PointerToLinenumbers
        {
            get;
            set;
        }

        public ushort NumberOfRelocations
        {
            get;
            set;
        }

        public ushort NumberOfLinenumbers
        {
            get;
            set;
        }

        public ImageSectionAttributes Attributes
        {
            get;
            set;
        }

        public ImageSection Section
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return 8 * sizeof(byte) +
                   6 * sizeof (uint) +
                   2 * sizeof (ushort) +
                   1 * sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            var nameBytes = Encoding.ASCII.GetBytes(Name ?? string.Empty);
            writer.WriteBytes(nameBytes);
            writer.WriteBytes(new byte[8 - nameBytes.Length]);

            writer.WriteUInt32(VirtualSize);
            writer.WriteUInt32(VirtualAddress);
            writer.WriteUInt32(SizeOfRawData);
            writer.WriteUInt32(PointerToRawData);
            writer.WriteUInt32(PointerToRelocations);
            writer.WriteUInt32(PointerToLinenumbers);
            writer.WriteUInt16(NumberOfRelocations);
            writer.WriteUInt16(NumberOfLinenumbers);
            writer.WriteUInt32((uint)Attributes);
        }

        public long RvaToFileOffset(long rva)
        {
            return rva - VirtualAddress + PointerToRawData;
        }

        public long FileOffsetToRva(long fileOffset)
        {
            return fileOffset - PointerToRawData + VirtualAddress;
        }


    }
}
