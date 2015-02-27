using System;

namespace AsmResolver.Net.Metadata
{
    [Flags]
    public enum AssemblyAttributes : uint
    {
        /// <summary>
        /// The assembly holds the full (unhashed) public key.
        /// </summary>
        PublicKey = 0x0001,
        /// <summary>
        /// The assembly uses an unspecified processor architecture.
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// The assembly uses a neutral pe32 processor architecture.
        /// </summary>
        Msil = 0x0010,
        /// <summary>
        /// The assembly uses a x86 pe32 processor architecture.
        /// </summary>
        X86 = 0x0020,
        /// <summary>
        /// The assembly uses an itanium pe32+ processor architecture.
        /// </summary>
        Ia64 = 0x0030,
        /// <summary>
        /// The assembly uses an AMD x64 pe32+ processor architecture.
        /// </summary>
        Amd64 = 0x0040,
        /// <summary>
        /// Propagate PA flags to Assembly record.
        /// </summary>
        Specified = 0x0080,
        /// <summary>
        /// Bits describing the processor architecture.
        /// </summary>
        Mask = 0x0070, // Bits describing the processor architecture
        FullMask = 0x00F0, // Bits describing the PA incl. Specified
        Shift = 0x0004, // NOT A FLAG, shift count in PA flags <--> index conversion
        /// <summary>
        /// From "DebuggableAttribute".
        /// </summary>
        EnableJitCompileTracking = 0x8000,
        /// <summary>
        /// From "DebuggableAttribute".
        /// </summary>
        DisableJitCompileOptimizer = 0x4000,
        /// <summary>
        /// The assembly can be retargeted (at runtime) to an assembly from a different publisher.
        /// </summary>
        Retargetable = 0x0100,
    }
}