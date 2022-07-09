using System;

namespace AsmResolver.DotNet.Bundles
{
    /// <summary>
    /// Provides members defining all flags that can be assigned to a bundle manifest.
    /// </summary>
    [Flags]
    public enum BundleManifestFlags : ulong
    {
        /// <summary>
        /// Indicates no flags were assigned.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates the bundle was compiled in .NET Core 3 compatibility mode.
        /// </summary>
        NetCoreApp3CompatibilityMode = 1
    }
}
