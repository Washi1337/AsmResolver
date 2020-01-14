namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a signature stored in the #Blob stream of a managed executable file.
    /// </summary>
    public abstract class BlobSignature : IWritable 
    {
        /// <inheritdoc />
        public abstract uint GetPhysicalSize();

        /// <inheritdoc />
        public abstract void Write(IBinaryStreamWriter writer);
    }
}