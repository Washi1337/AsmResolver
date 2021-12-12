using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Blob
{
    /// <summary>
    /// Represents the metadata stream containing blob signatures referenced by entries in the tables stream.
    /// </summary>
    /// <remarks>
    /// Like most metadata streams, the blob stream does not necessarily contain just valid blobs. It can contain
    /// (garbage) data that is never referenced by any of the tables in the tables stream. The only guarantee that the
    /// blob heap provides, is that any blob index in the tables stream is the start address (relative to the start of
    /// the blob stream) of a blob signature that is prefixed by a length.
    /// </remarks>
    public abstract class BlobStream : MetadataHeap
    {
        /// <summary>
        /// The default name of a blob stream, as described in the specification provided by ECMA-335.
        /// </summary>
        public const string DefaultName = "#Blob";

        /// <summary>
        /// Initializes the blob stream with its default name.
        /// </summary>
        protected BlobStream()
            : base(DefaultName)
        {
        }

        /// <summary>
        /// Initializes the blob stream with a custom name.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        protected BlobStream(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets a blob by its blob index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>
        /// The blob, excluding the bytes encoding the length of the blob, or <c>null</c> if the index was invalid.
        /// </returns>
        public abstract byte[]? GetBlobByIndex(uint index);

        /// <summary>
        /// Gets a blob binary reader by its blob index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <param name="reader">When this method returns <c>true</c>, this parameter contains the created binary reader.</param>
        /// <returns>
        /// <c>true</c> if a blob reader could be created at the provided index, <c>false</c> otherwise.
        /// </returns>
        public abstract bool TryGetBlobReaderByIndex(uint index, out BinaryStreamReader reader);

        /// <summary>
        /// Searches the stream for the provided blob.
        /// </summary>
        /// <param name="blob">The blob to search for.</param>
        /// <param name="index">When the function returns <c>true</c>, contains the index at which the blob was found..</param>
        /// <returns><c>true</c> if the blob index was found, <c>false</c> otherwise.</returns>
        public abstract bool TryFindBlobIndex(byte[] blob, out uint index);
    }
}
