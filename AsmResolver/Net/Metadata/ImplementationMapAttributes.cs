using System;

namespace AsmResolver.Net.Metadata
{
    [Flags]
    public enum ImplementationMapAttributes : ushort
    {
        NoMangle = 0x0001,
        CharSetMask = 0x0006,
        CharSetNotSpec = 0x0000,
        CharSetAnsi = 0x0002,
        CharSetUnicode = 0x0004,
        CharSetAuto = 0x0006,
        BestFitUseAssem = 0x0000,
        BestFitEnabled = 0x0010,
        BestFitDisabled = 0x0020,
        BestFitMask = 0x0030,
        ThrowOnUnmappableCharUseAssem = 0x0000,
        ThrowOnUnmappableCharEnabled = 0x1000,
        ThrowOnUnmappableCharDisabled = 0x2000,
        ThrowOnUnmappableCharMask = 0x3000,
        SupportsLastError = 0x0040,
        CallConvMask = 0x0700,
        CallConvWinapi = 0x0100,
        CallConvCdecl = 0x0200,
        CallConvStdcall = 0x0300,
        CallConvThiscall = 0x0400,
        CallConvFastcall = 0x0500,
    }
}