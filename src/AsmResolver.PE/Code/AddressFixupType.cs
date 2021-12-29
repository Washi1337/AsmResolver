namespace AsmResolver.PE.Code
{
    /// <summary>
    /// Defines all possible address fixup types that can be applied in a <see cref="CodeSegment"/>.
    /// </summary>
    public enum AddressFixupType
    {
        /// <summary>
        /// Indicates the fixup writes a 32-bit absolute address at the provided offset.
        /// </summary>
        Absolute32BitAddress,

        /// <summary>
        /// Indicates the fixup writes a 64-bit absolute address at the provided offset.
        /// </summary>
        Absolute64BitAddress,

        /// <summary>
        /// Indicates the fixup writes a 32-bit address relative to the instruction pointer at the provided offset.
        /// </summary>
        Relative32BitAddress,
    }
}
