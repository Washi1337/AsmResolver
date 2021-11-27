using AsmResolver.IO;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents the COFF file header of a portable executable file.
    /// </summary>
    public class FileHeader : SegmentBase
    {
        /// <summary>
        /// Indicates the static size of the COFF file header.
        /// </summary>
        public const int FileHeaderSize = 2 * sizeof (ushort) +
                                          3 * sizeof (uint) +
                                          2 * sizeof (ushort);

        /// <summary>
        /// Reads a COFF file header from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read file header.</returns>
        public static FileHeader FromReader(ref BinaryStreamReader reader) => new()
        {
            Offset = reader.Offset,
            Rva = reader.Rva,
            Machine = (MachineType) reader.ReadUInt16(),
            NumberOfSections = reader.ReadUInt16(),
            TimeDateStamp = reader.ReadUInt32(),
            PointerToSymbolTable = reader.ReadUInt32(),
            NumberOfSymbols = reader.ReadUInt32(),
            SizeOfOptionalHeader = reader.ReadUInt16(),
            Characteristics = (Characteristics) reader.ReadUInt16()
        };

        /// <summary>
        /// Gets or sets the machine the portable executable file is compiled for.
        /// </summary>
        public MachineType Machine
        {
            get;
            set;
        } = MachineType.I386;

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
        /// <remarks>
        /// This timestamp is encoded as the number of seconds since 00:00 January 1, 1970.
        /// </remarks>
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
        } = 0xE0;

        /// <summary>
        /// Gets or sets the characteristics of the portable executable file.
        /// </summary>
        public Characteristics Characteristics
        {
            get;
            set;
        } = Characteristics.Image | Characteristics.Machine32Bit;

        /// <inheritdoc />
        public override uint GetPhysicalSize() => FileHeaderSize;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
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
