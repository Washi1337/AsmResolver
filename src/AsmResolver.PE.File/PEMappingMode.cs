namespace AsmResolver.PE.File
{
    /// <summary>
    /// Provides members for describing whether a portable executable file is in its mapped or unmapped form.
    /// </summary>
    public enum PEMappingMode
    {
        /// <summary>
        /// Indicates the portable executable is in its unmapped form.
        /// Every offset in the portable executable refers to a physical file offset as it would appear on disk.
        /// </summary>
        Unmapped,
        
        /// <summary>
        /// Indicates the portable executable is in its mapped form.
        /// Every offset in the portable executable refers to an absolute memory address.
        /// </summary>
        Mapped
    }
}