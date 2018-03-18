namespace AsmResolver
{
    /// <summary>
    /// Represents the file header of a windows assembly image.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the machine the image is compiled for.
        /// </summary>
        public ImageMachineType Machine
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of sections defined in the image.
        /// </summary>
        public ushort NumberOfSections
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time stamp of the image.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the absolute file offset to the symbol table.
        /// </summary>
        public uint PointerToSymbolTable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of symbols defined in the image.
        /// </summary>
        public uint NumberOfSymbols
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the size of the optional header.
        /// </summary>
        public ushort SizeOfOptionalHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the characteristics of the image.
        /// </summary>
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
