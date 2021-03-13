using System;

namespace AsmResolver.DotNet.Memory
{
    [Flags]
    public enum MemoryLayoutAttributes
    {
        Is32Bit = 0b0,
        Is64Bit = 0b1,
        BitnessMask = 0b1,

        IsPlatformDependent = 0b10,
    }
}
