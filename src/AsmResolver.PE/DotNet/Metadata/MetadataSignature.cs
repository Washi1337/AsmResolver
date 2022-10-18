namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Provides members defining all possible signatures that a metadata directory can start with.
    /// </summary>
    public enum MetadataSignature
    {
        /// <summary>
        /// Indicates the BSJB metadata directory format is used.
        /// </summary>
        Bsjb = 0x424A5342,

        /// <summary>
        /// Indicates the legacy COM+ metadata directory format is used.
        /// </summary>
        ComPlus = 0x2B4D4F43
    }
}
