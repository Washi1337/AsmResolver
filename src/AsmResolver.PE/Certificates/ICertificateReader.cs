using AsmResolver.IO;

namespace AsmResolver.PE.Certificates
{
    /// <summary>
    /// Provides members for reading and interpreting the contents of an attribute certificate in a signed
    /// portable executable file.
    /// </summary>
    public interface ICertificateReader
    {
        /// <summary>
        /// Reads a single attribute certificate.
        /// </summary>
        /// <param name="context">The context in which the reader is situated in.</param>
        /// <param name="revision">The file format revision to use.</param>
        /// <param name="type">The type of certificate to read.</param>
        /// <param name="reader">The reader pointing to the start of the raw data of the certificate.</param>
        /// <returns>The read attribute certificate.</returns>
        AttributeCertificate ReadCertificate(
            PEReaderContext context,
            CertificateRevision revision,
            CertificateType type,
            BinaryStreamReader reader);
    }
}
