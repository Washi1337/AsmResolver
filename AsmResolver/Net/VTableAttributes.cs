using System;

namespace AsmResolver.Net
{
    [Flags]
    public enum VTableAttributes : ushort
    {
        Is32Bit = 0x1,

        Is64Bit = 0x2,

        FromUnmanaged = 0x4,

        CallMostDerived = 0x10,
    }
}