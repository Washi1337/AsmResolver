using AsmResolver.DotNet.Builder;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a blob signature that might contain extra data not part of the standard format of the signature.
    /// </summary>
    public abstract class ExtendableBlobSignature : BlobSignature
    {
        /// <summary>
        /// Gets or sets the extra custom data in the blob signature.
        /// </summary>
        public byte[] ExtraData
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        public sealed override void Write(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider)
        {
            WriteContents(writer, provider);
            if (ExtraData != null)
                writer.WriteBytes(ExtraData);
        }

        /// <summary>
        /// Serializes the blob (without extra data) to an output stream.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        /// <param name="provider">The object to use for obtaining metadata tokens for members in the tables stream.</param>
        protected abstract void WriteContents(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider);
    }
}