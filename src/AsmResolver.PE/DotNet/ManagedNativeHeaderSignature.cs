namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Provides members defining all possible managed native header types.
    /// </summary>
    public enum ManagedNativeHeaderSignature : uint
    {
        /// <summary>
        /// Indicates the managed native header is in the Native Image Generator (NGEN) file format.
        /// </summary>
        NGen = 0x4e45474e,

        /// <summary>
        /// Indicates the managed native header is in the ReadyToRun (RTR) file format.
        /// </summary>
        Rtr = 0x00525452
    }
}
