using System.Diagnostics;
using AsmResolver.IO;

namespace AsmResolver.PE.Certificates
{
    /// <summary>
    /// Represents an attribute certificate that contains a custom or unsupported certificate type or file format.
    /// </summary>
    [DebuggerDisplay("{Type}")]
    public class CustomAttributeCertificate : AttributeCertificate
    {
        /// <summary>
        /// Creates a new custom attribute certificate with the provided contents.
        /// </summary>
        /// <param name="type">The certificate type to store.</param>
        /// <param name="contents">The certificate data.</param>
        public CustomAttributeCertificate(CertificateType type, IReadableSegment? contents)
            : this(CertificateRevision.Revision_v2_0, type, contents)
        {
        }

        /// <summary>
        /// Creates a new custom attribute certificate with the provided contents.
        /// </summary>
        /// <param name="revision">The revision of the file format to use.</param>
        /// <param name="type">The certificate type to store.</param>
        /// <param name="contents">The certificate data.</param>
        public CustomAttributeCertificate(CertificateRevision revision, CertificateType type, IReadableSegment? contents)
        {
            Revision = revision;
            Type = type;
            Contents = contents;
        }

        /// <inheritdoc />
        public override CertificateRevision Revision
        {
            get;
        }

        /// <inheritdoc />
        public override CertificateType Type
        {
            get;
        }

        /// <summary>
        /// Gets or sets the raw contents of the certificate.
        /// </summary>
        public IReadableSegment? Contents
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(in parameters);
            Contents?.UpdateOffsets(parameters.WithAdvance(HeaderSize));
        }

        /// <inheritdoc />
        protected override uint GetContentsSize() => Contents?.GetPhysicalSize() ?? 0;

        /// <inheritdoc />
        protected override void WriteContents(IBinaryStreamWriter writer) => Contents?.Write(writer);
    }
}
