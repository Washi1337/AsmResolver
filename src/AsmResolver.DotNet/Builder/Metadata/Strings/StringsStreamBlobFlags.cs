using System;

namespace AsmResolver.DotNet.Builder.Metadata.Strings
{
    [Flags]
    internal enum StringsStreamBlobFlags : byte
    {
        ZeroTerminated = 1,
        Fixed = 2
    }
}
