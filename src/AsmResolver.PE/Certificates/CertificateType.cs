namespace AsmResolver.PE.Certificates
{
    /// <summary>
    /// Provides members describing all possible certificate types a single attribute certificate in a signed portable
    /// executable file can contain.
    /// </summary>
    public enum CertificateType : ushort
    {
        /// <summary>
        /// Indicates the contents of the attribute is a X.509 certificate.
        /// </summary>
        X509 = 1,

        /// <summary>
        /// Indicates the contents of the attribute is a PKCS#7 SignedData structure.
        /// </summary>
        PkcsSignedData = 2,

        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved1 = 3,

        /// <summary>
        /// Indicates the contents of the attribute is a Terminal Server Protocol stack certificate.
        /// </summary>
        TsStackSigned = 4
    }
}
