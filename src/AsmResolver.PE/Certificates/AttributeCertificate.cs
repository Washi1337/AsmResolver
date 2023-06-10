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

        /// <inheritdoc />
        public override uint GetPhysicalSize() => HeaderSize + GetContentsSize();

        /// <summary>
        /// Gets the total size in bytes of the certificate contents itself, excluding the header.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        protected abstract uint GetContentsSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
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
        protected abstract void WriteContents(IBinaryStreamWriter writer);
    }
}
