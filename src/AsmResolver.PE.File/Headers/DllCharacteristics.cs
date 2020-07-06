using System;

namespace AsmResolver.PE.File.Headers
{
    [Flags]
    public enum DllCharacteristics
    {
        HighEntropyVA  = 0x0020,
        DynamicBase = 0x0040,
        ForceIntegrity = 0x0080,
        NxCompat = 0x0100,
        NoIsolation = 0x0200,
        NoSeh = 0x0400,
        NoBind = 0x0800,
        WdmDriver = 0x2000,
        TerminalServerAware = 0x8000,
    }
}