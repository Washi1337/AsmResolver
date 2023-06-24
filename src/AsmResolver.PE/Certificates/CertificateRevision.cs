namespace AsmResolver.PE.Certificates
{
    /// <summary>
    /// Provides members describing all possible file format revisions that can be used in an attribute certificate entry.
    /// </summary>
    public enum CertificateRevision : ushort
    {
        /// <summary>
        /// Indicates version 1.0 of the file format is used.
        /// </summary>
        Revision_v1_0 = 0x0100,

        /// <summary>
        /// Indicates version 2.0 of the file format is used.
        /// </summary>
        Revision_v2_0 = 0x0200
    }
}
