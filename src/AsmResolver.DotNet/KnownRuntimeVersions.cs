namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides strings of known runtime version numbers. 
    /// </summary>
    public static class KnownRuntimeVersions
    {
        // String values are taken from the original sscli (rotor) documentation files (sscli/docs/relnotes.html),
        // and from sample binaries.
        
        /// <summary>
        /// Indicates the ECMA 2002 runtime version string.
        /// </summary>
        public const string Ecma2002 = "Standard CLI 2002";
        
        /// <summary>
        /// Indicates the ECMA 2005 runtime version string.
        /// </summary>
        public const string Ecma2005 = "Standard CLI 2005";

        /// <summary>
        /// Indicates the Microsoft .NET Framework 1.0 runtime version string.
        /// </summary>
        public const string Clr10 = "v1.0.3705";
        
        /// <summary>
        /// Indicates the Microsoft .NET Framework 1.1 runtime version string.
        /// </summary>
        public const string Clr11 = "v1.1.4322";
        
        /// <summary>
        /// Indicates the Microsoft .NET Framework 2.0 runtime version string.
        /// </summary>
        public const string Clr20 = "v2.0.50727";

        /// <summary>
        /// Indicates the Microsoft .NET Framework 4.0 runtime version string.
        /// </summary>
        public const string Clr40 = "v4.0.30319";
    }
}