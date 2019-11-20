namespace AsmResolver.DotNet.Blob
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
        public override uint GetPhysicalSize()
        {
            return (uint) (ExtraData?.Length ?? 0);
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            if (ExtraData != null)
                writer.WriteBytes(ExtraData);
        }
    }
}