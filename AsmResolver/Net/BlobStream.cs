namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a blob storage stream (#Blob) in a .NET assembly image.
    /// </summary>
    public class BlobStream : MetadataStream
    {
        internal static BlobStream FromReadingContext(ReadingContext context)
        {
            return new BlobStream(context.Reader);
        }

        private readonly IBinaryStreamReader _reader;

        public BlobStream()
        {
        }

        internal BlobStream(IBinaryStreamReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Gets the blob at the given index.
        /// </summary>
        /// <param name="offset">The index of the blob to get.</param>
        /// <returns>The raw blob data.</returns>
        public byte[] GetBlobByOffset(uint offset)
        {
            if (offset == 0)
                return new byte[0];

            var reader = CreateBlobReader(offset);
            return reader.ReadBytes((int)reader.Length);
        }
        
        /// <summary>
        /// Tries to create a new blob reader starting at the given offset.
        /// </summary>
        /// <param name="offset">The index of the blob to read.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>True</c> if the reader was created successfully, false otherwise.</returns>
        public bool TryCreateBlobReader(uint offset, out IBinaryStreamReader reader)
        {
            try
            {
                reader = CreateBlobReader(offset);
                return true;
            }
            catch
            {
                reader = null;
                return false;
            }
        }

        /// <summary>
        /// Creates a new blob reader starting at the given offset.
        /// </summary>
        /// <param name="offset">The index of the blob to read.</param>
        /// <returns>The blob reader.</returns>
        public IBinaryStreamReader CreateBlobReader(uint offset)
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition + offset);
            uint length;
            reader.TryReadCompressedUInt32(out length);
            return reader.CreateSubReader(reader.Position, (int)length);
        }

        public override uint GetPhysicalLength()
        {
            return (uint)_reader.Length;
        }

        public override void Write(WritingContext context)
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition, (int) _reader.Length);
            context.Writer.WriteBytes(reader.ReadBytes((int) reader.Length));
        }
    }

    
}
