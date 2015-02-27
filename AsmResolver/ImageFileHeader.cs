using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public class ImageFileHeader : FileSegment
    {
        internal static ImageFileHeader FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            return new ImageFileHeader()
            {
                StartOffset = reader.Position,
                Machine = (ImageMachineType)reader.ReadUInt16(),
                NumberOfSections = reader.ReadUInt16(),
                TimeDateStamp = reader.ReadUInt32(),
                PointerToSymbolTable = reader.ReadUInt32(),
                NumberOfSymbols = reader.ReadUInt32(),
                SizeOfOptionalHeader = reader.ReadUInt16(),
                Characteristics = (ImageCharacteristics)reader.ReadUInt16(),
            };
        }

        public ImageMachineType Machine
        {
            get;
            set;
        }

        public ushort NumberOfSections
        {
            get;
            set;
        }

        public uint TimeDateStamp
        {
            get;
            set;
        }

        public uint PointerToSymbolTable
        {
            get;
            set;
        }

        public uint NumberOfSymbols
        {
            get;
            set;
        }

        public ushort SizeOfOptionalHeader
        {
            get;
            set;
        }

        public ImageCharacteristics Characteristics
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return 2 * sizeof (ushort) +
                   3 * sizeof (uint) +
                   2 * sizeof (ushort);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt16((ushort)Machine);
            writer.WriteUInt16(NumberOfSections);
            writer.WriteUInt32(TimeDateStamp);
            writer.WriteUInt32(PointerToSymbolTable);
            writer.WriteUInt32(NumberOfSymbols);
            writer.WriteUInt16(SizeOfOptionalHeader);
            writer.WriteUInt16((ushort)Characteristics);
        }
    }
}
