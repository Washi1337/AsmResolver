namespace AsmResolver.PE.File
{
    /// <summary>
    /// Represents the COFF file header of a portable executable file.
    /// </summary>
    public class ImageFileHeader : ISegment
    {
        public static ImageFileHeader FromReader(IBinaryStreamReader reader)
        {
            return new ImageFileHeader
            {
                FileOffset = reader.FileOffset,
                Machine = (ImageMachineType) reader.ReadUInt16(),
                NumberOfSections = reader.ReadUInt16(),
                TimeDateStamp = reader.ReadUInt32(),
                PointerToSymbolTable = reader.ReadUInt32(),
                NumberOfSymbols = reader.ReadUInt32(),
                SizeOfOptionalHeader = reader.ReadUInt16(),
                Characteristics = (ImageCharacteristics) reader.ReadUInt16()
            };
        }

        public ImageFileHeader()
        {
        }
        
          /// <summary>
        /// Gets or sets the machine the portable executable file is compiled for.
        /// </summary>
        public ImageMachineType Machine
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of sections defined in the portable executable file.
        /// </summary>
        public ushort NumberOfSections
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time stamp of the portable executable file.
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
        /// Gets or sets the number of symbols defined in the portable executable file.
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
        /// Gets or sets the characteristics of the portable executable file.
        /// </summary>
        public ImageCharacteristics Characteristics
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        uint ISegment.Rva => FileOffset;

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return 2 * sizeof (ushort) +
                   3 * sizeof (uint) +
                   2 * sizeof (ushort);
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return GetPhysicalSize();
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
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