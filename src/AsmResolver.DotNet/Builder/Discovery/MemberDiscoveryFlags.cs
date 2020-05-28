using System;

namespace AsmResolver.DotNet.Builder.Discovery
{
    [Flags]
    public enum MemberDiscoveryFlags
    {
        None = 0,
        PreserveTypeOrder = 1,
        PreserveFieldOrder = 2,
        PreserveMethodOrder = 4,
        PreserveParameterOrder = 8,
        PreserveEventOrder = 16,
        PreservePropertyOrder = 32
    }
}