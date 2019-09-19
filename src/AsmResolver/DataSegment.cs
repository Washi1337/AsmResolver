namespace AsmResolver
{
    /// <summary>
    /// Provides an implementation of a segment using a byte array to represent its contents. 
    /// </summary>
    public class DataSegment : ISegment
    {
        /// <summary>
        /// Puts the remaining data of the provided input stream into a new data segment. 
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The data segment containing the remaining data.</returns>
        public static DataSegment FromReader(IBinaryStreamReader reader)
        {
            return FromReader(reader, (int) (reader.StartPosition + reader.Length - reader.FileOffset));
        }
        
        /// <summary>
        /// Reads a single data segment at the current position of the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The read data segment.</returns>
        public static DataSegment FromReader(IBinaryStreamReader reader, int count)
        {
            uint position = reader.FileOffset;
            uint rva = reader.Rva;
            
            var buffer = new byte[count];
            reader.ReadBytes(buffer, 0, count);
            
            return new DataSegment(buffer)
            {
                FileOffset = position,
                Rva = rva
            };
        }
        
        /// <summary>
        /// Creates a new data segment using the provided byte array as contents.
        /// </summary>
        /// <param name="data">The data to store.</param>
        public DataSegment(byte[] data)
        {
            Data = data;
        }

        /// <inheritdoc />
        public uint FileOffset
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            private set;
        }

        public byte[] Data
        {
            get;
        }

        public void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
            Rva = newRva;
        }

        /// <inheritdoc />
        public uint GetPhysicalSize()
        {
            return (uint) Data.Length;
        }

        /// <inheritdoc />
        public uint GetVirtualSize()
        {
            return (uint) Data.Length;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBytes(Data, 0, Data.Length);
        }
        
    }
}