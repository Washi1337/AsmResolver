namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents a data directory header, consisting of the starting address, and the size.
    /// </summary>
    /// <remarks>
    /// This class does not contain the actual contents of the data directory.
    /// </remarks>
    public class DataDirectory : ISegment
    {
        private uint _fileOffset;
        private uint _rva;

        /// <summary>
        /// Reads a single data directory at the current position of the provided input stream. 
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The data directory that was read.</returns>
        internal static DataDirectory FromReader(IBinaryStreamReader reader)
        {
            uint offset = reader.FileOffset;
            uint rva = reader.Rva;
            return new DataDirectory(reader.ReadUInt32(), reader.ReadUInt32())
            {
                _fileOffset = offset,
                _rva = rva,
            };
        }

        /// <summary>
        /// Creates a new data directory header.
        /// </summary>
        /// <param name="virtualAddress">The starting virtual address (RVA) of the directory.</param>
        /// <param name="size">The size in bytes of the directory.</param>
        public DataDirectory(uint virtualAddress, uint size)
        {
            VirtualAddress = virtualAddress;
            Size = size;
        }

        /// <inheritdoc />
        uint ISegment.FileOffset => _fileOffset;

        /// <inheritdoc />
        uint ISegment.Rva => _rva;

        /// <summary>
        /// Gets or sets the relative virtual address (RVA) of the directory.
        /// </summary>
        public uint VirtualAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the directory.
        /// </summary>
        public uint Size
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            _fileOffset = newFileOffset; 
            _rva = newRva;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return 2 * sizeof (uint);
        }
        /// <inheritdoc />

        public uint GetVirtualSize()
        {
            return GetPhysicalSize();
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(VirtualAddress);
            writer.WriteUInt32(Size);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"RVA: 0x{VirtualAddress:X8}, Size: 0x{Size:X8}";
        }
        
    }
}