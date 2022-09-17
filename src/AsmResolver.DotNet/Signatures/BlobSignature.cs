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
        public abstract void Write(in BlobSerializationContext context);

        /// <summary>
        /// Wraps the blob signature into a new stand-alone signature that can be referenced by a metadata token.
        /// </summary>
        /// <returns>The new stand-alone signature.</returns>
        public StandAloneSignature MakeStandAloneSignature() => new(this);
    }
}
