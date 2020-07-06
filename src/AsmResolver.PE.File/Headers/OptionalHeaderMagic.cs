namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Provides valid values for the optional header magic.
    /// </summary>
    public enum OptionalHeaderMagic : ushort
    {
        /// <summary>
        /// Indicates the binary contains a 32-bit portable executable file.
        /// </summary>
        Pe32 = 0x010B,
        
        /// <summary>
        /// Indicates the binary contains a 64-bit portable executable file.
        /// </summary>
        Pe32Plus = 0x020B,
    }
}