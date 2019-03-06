using AsmResolver.Net.Emit;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Provides a base for all signatures found in the #Blob metadata stream.
    /// </summary>
    public abstract class BlobSignature
    {
        /// <summary>
        /// Gets the physical amount of bytes needed to store the signature in the provided metadata buffer.
        /// </summary>
        /// <param name="buffer">The metadata buffer to add the signature to.</param>
        /// <returns>The amount of bytes.</returns>
        public abstract uint GetPhysicalLength(MetadataBuffer buffer);

        /// <summary>
        /// Prepares the blob signature for adding to the metadata buffer. This is required by some signatures as they
        /// might have to add references to embedded type signatures to the metadata buffer. 
        /// </summary>
        /// <param name="buffer">The metadata buffer to add the signature to.</param>
        public abstract void Prepare(MetadataBuffer buffer);
        
        /// <summary>
        /// Serializes the blob signature to the provided output stream.
        /// </summary>
        /// <param name="buffer">The metadata buffer to add the signature to.</param>
        /// <param name="writer">The writer to use.</param>
        public abstract void Write(MetadataBuffer buffer, IBinaryStreamWriter writer);
    }

    /// <summary>
    /// Represents a blob stream that might contain extra data not part of the standard format of the signature.
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
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (ExtraData?.Length ?? 0);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            if (ExtraData != null)
                writer.WriteBytes(ExtraData);
        }
    }
}
