namespace AsmResolver.Net.Metadata
{
    public enum AssemblyHashAlgorithm : uint
    {
        /// <summary>
        /// No assembly hash algorithm is being used.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The md5 hash algorithm is being used.
        /// </summary>
        Md5 = 0x8003,
        /// <summary>
        /// The sha1 hash algorithm is being used.
        /// </summary>
        Sha1 = 0x8004
    }
}