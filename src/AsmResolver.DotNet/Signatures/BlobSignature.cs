using AsmResolver.DotNet.Builder;

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
        /// <param name="writer">The output stream.</param>
        /// <param name="provider">The object to use for obtaining metadata tokens for members in the tables stream.</param>
        public abstract void Write(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider);
    }
}