namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a signature stored in the #Blob stream of a managed executable file.
    /// </summary>
    public abstract class BlobSignature
    {
        /// <summary>
        /// Serializes the blob to an output stream.
        /// </summary>
        public abstract void Write(BlobSerializationContext context);
    }
}