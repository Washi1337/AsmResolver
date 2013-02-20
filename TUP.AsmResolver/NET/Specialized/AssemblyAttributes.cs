using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable 1591

namespace TUP.AsmResolver.NET.Specialized
{
    [Flags]
    public enum AssemblyAttributes : uint
    {
        PublicKey = 0x0001,     // The assembly ref holds the full (unhashed) public key.

        None = 0x0000,     // Processor Architecture unspecified
        MSIL = 0x0010,     // Processor Architecture: neutral (PE32)
        x86 = 0x0020,     // Processor Architecture: x86 (PE32)
        IA64 = 0x0030,     // Processor Architecture: Itanium (PE32+)
        AMD64 = 0x0040,     // Processor Architecture: AMD X64 (PE32+)
        Specified = 0x0080,     // Propagate PA flags to AssemblyRef record
        Mask = 0x0070,     // Bits describing the processor architecture
        FullMask = 0x00F0,     // Bits describing the PA incl. Specified
        Shift = 0x0004,     // NOT A FLAG, shift count in PA flags <--> index conversion

        EnableJITcompileTracking = 0x8000, // From "DebuggableAttribute".
        DisableJITcompileOptimizer = 0x4000, // From "DebuggableAttribute".

        Retargetable = 0x0100,     // The assembly can be retargeted (at runtime) to an
                                    // assembly from a different publisher.
    }
}
