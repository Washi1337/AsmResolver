namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Represents a data directory header, consisting of the starting address, and the size.
    /// </summary>
    /// <remarks>
    /// This structure does not contain the actual contents of the data directory.
    /// </remarks>
    public class DataDirectory : IWritable
    {
        /// <summary>
        /// Indicates the size of a single data directory header.
        /// </summary>
        public const uint DataDirectorySize = 2 * sizeof(uint);
        
        /// <summary>
        /// Reads a single data directory at the current position of the provided input stream. 
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The data directory that was read.</returns>
        public static DataDirectory FromReader(IBinaryStreamReader reader)
        {
            return new DataDirectory(reader.ReadUInt32(), reader.ReadUInt32());
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

        /// <summary>
        /// Gets a value indicating the data directory is present in the portable executable file. 
        /// </summary>
        public bool IsPresentInPE => VirtualAddress != 0 || Size != 0;

        /// <inheritdoc />
        public uint GetPhysicalSize() => DataDirectorySize;

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