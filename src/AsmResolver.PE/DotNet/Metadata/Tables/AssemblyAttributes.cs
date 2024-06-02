using System;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides members defining all possible flags that can be assigned to an assembly definition or reference.
    /// </summary>
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
        ArchitectureNone = 0x0000,
        /// <summary>
        /// The assembly uses a neutral processor architecture.
        /// </summary>
        ArchitectureMsil = 0x0010,
        /// <summary>
        /// The assembly uses a x86 pe32 processor architecture.
        /// </summary>
        ArchitectureX86 = 0x0020,
        /// <summary>
        /// The assembly uses an itanium pe32+ processor architecture.
        /// </summary>
        ArchitectureIa64 = 0x0030,
        /// <summary>
        /// The assembly uses an AMD x64 pe32+ processor architecture.
        /// </summary>
        ArchitectureAmd64 = 0x0040,
        /// <summary>
        /// Bits describing the processor architecture.
        /// </summary>
        ArchitectureMask = 0x0070,
        /// <summary>
        /// Propagate PA flags to Assembly record.
        /// </summary>
        Specified = 0x0080,
        /// <summary>
        /// Bits describing the PA incl. Specified.
        /// </summary>
        FullMask = 0x00F0,
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
        /// <summary>
        /// The assembly contains .NET Framework code.
        /// </summary>
        ContentDefault         = 0x0000,
        /// <summary>
        /// The assembly contains Windows Runtime code.
        /// </summary>
        ContentWindowsRuntime  = 0x0200,
        /// <summary>
        /// Bits describing the content type.
        /// </summary>
        ContentMask            = 0x0E00
    }
}
