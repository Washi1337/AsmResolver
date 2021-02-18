namespace AsmResolver.PE.Debug.CodeView
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
        /// NB10 Format Signature
        /// </summary>
        Nb10 = 0x3031424e
    }
}
