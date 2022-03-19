using System;

namespace AsmResolver.PE.DotNet.Bundles
{
    [Flags]
    public enum BundleManifestFlags : ulong
    {
        None = 0,
        NetCoreApp3CompatibilityMode = 1
    }
}
