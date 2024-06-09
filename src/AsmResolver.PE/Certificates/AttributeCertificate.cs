using AsmResolver.IO;

namespace AsmResolver.PE.Certificates
{
    /// <summary>
    /// When derived from this class, represents a single certificate in the attribute certificate data directory of
    /// a signed portable executable file.
    /// </summary>
    public abstract class AttributeCertificate : SegmentBase
    {
        /// <summary>
        /// Defines the size of a single header of a certificate.
        /// </summary>
        public const uint HeaderSize =
                sizeof(uint) // dwLength
                + sizeof(ushort) // wRevision
                + sizeof(ushort) // wCertificateType
            ;

        /// <summary>
        /// Gets the revision of the file format that is used for this certificate.
        /// </summary>
        public abstract CertificateRevision Revision
        {
            get;
        }

        /// <summary>
        /// Gets the type of the certificate.
        /// </summary>
        public abstract CertificateType Type
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the raw contents of the signature can be read using a
        /// <see cref="BinaryStreamReader"/> instance.
        /// </summary>
        public abstract bool CanRead
        {
            get;
        }

        /// <summary>
        /// Creates a binary reader that reads the raw contents of the stored signature.
        /// </summary>
        /// <returns>The reader.</returns>
        public abstract BinaryStreamReader CreateContentReader();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => HeaderSize + GetContentsSize();

        /// <summary>
        /// Gets the total size in bytes of the certificate contents itself, excluding the header.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        protected abstract uint GetContentsSize();

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            writer.WriteUInt32(GetPhysicalSize());
            writer.WriteUInt16((ushort) Revision);
            writer.WriteUInt16((ushort) Type);
            WriteContents(writer);
        }

        /// <summary>
        /// Writes the contents of the certificate to the provided output stream.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        protected abstract void WriteContents(BinaryStreamWriter writer);
    }
}
