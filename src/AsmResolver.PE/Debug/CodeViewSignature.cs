namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Enum of the different CodeView Signatures
    /// </summary>
    public enum CodeViewSignature
    {
        /// <summary>
        /// RSDS Format Signature
        /// </summary>
        Rsds = 0x53445352,

        /// <summary>
        /// NB05 Format Signature
        /// </summary>
        Nb05 = 0x3530424e,

        /// <summary>
        /// NB09 Format Signature
        /// </summary>
        Nb09 = 0x3930424e,

        /// <summary>
        /// NB10 Format Signature
        /// </summary>
        Nb10 = 0x3031424e,

        /// <summary>
        /// NB11 Format Signature
        /// </summary>
        Nb11 = 0x3131424e
    }
}
