using AsmResolver.IO;

namespace AsmResolver.PE.Certificates
{
    /// <summary>
    /// Provides a default implementation of an attribute certificate reader. This reader defaults to instantiating
    /// <see cref="CustomAttributeCertificate"/> if an unknown or unsupported file format is encountered.
    /// </summary>
    public class DefaultCertificateReader : ICertificateReader
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="DefaultCertificateReader"/> class.
        /// </summary>
        public static DefaultCertificateReader Instance { get; } = new();

        /// <inheritdoc />
        public AttributeCertificate ReadCertificate(
            PEReaderContext context,
            CertificateRevision revision,
            CertificateType type,
            BinaryStreamReader reader)
        {
            return new CustomAttributeCertificate(revision, type, reader.ReadSegment(reader.Length));
        }
    }
}
