using System;

namespace AsmResolver.PE.File.Headers
{
    /// <summary>
    /// Provides valid attributes for describing a portable executable file.
    /// </summary>
    [Flags]
    public enum Characteristics : ushort
    {
        RelocsStripped = 0x0001,
        Image = 0x0002,
        LineNumsStripped = 0x0004,
        LocalSymsStripped = 0x0008,
        AggressiveWsTrim = 0x0010,
        LargeAddressAware = 0x0020,
        Machine32Bit = 0x0100,
        DebugStripped = 0x0200,
        RemovableRunFromSwap = 0x0400,
        NetRunFromSwap = 0x0800,
        System = 0x1000,
        Dll = 0x2000,
        UpSystemOnly = 0x4000,
    }
}