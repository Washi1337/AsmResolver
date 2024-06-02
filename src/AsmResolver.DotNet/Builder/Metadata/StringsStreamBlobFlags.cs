using System;

namespace AsmResolver.DotNet.Builder.Metadata
{
    [Flags]
    internal enum StringsStreamBlobFlags : byte
    {
        ZeroTerminated = 1,
        Fixed = 2
    }
}
