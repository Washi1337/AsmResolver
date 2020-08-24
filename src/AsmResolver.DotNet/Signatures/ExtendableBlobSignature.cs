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
        public sealed override void Write(BlobSerializationContext context)
        {
            WriteContents(context);
            if (ExtraData != null)
                context.Writer.WriteBytes(ExtraData);
        }

        /// <summary>
        /// Serializes the blob (without extra data) to an output stream.
        /// </summary>
        protected abstract void WriteContents(BlobSerializationContext context);
    }
}