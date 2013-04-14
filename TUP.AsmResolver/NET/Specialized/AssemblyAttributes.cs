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
        MSIL = 0x0010,
        /// <summary>
        /// The assembly uses a x86 pe32 processor architecture.
        /// </summary>
        x86 = 0x0020,
        /// <summary>
        /// The assembly uses an itanium pe32+ processor architecture.
        /// </summary>
        IA64 = 0x0030,
        /// <summary>
        /// The assembly uses an AMD x64 pe32+ processor architecture.
        /// </summary>
        AMD64 = 0x0040, 
        /// <summary>
        /// Propagate PA flags to Assembly record.
        /// </summary>
        Specified = 0x0080,   
        /// <summary>
        /// Bits describing the processor architecture.
        /// </summary>
        Mask = 0x0070,     // Bits describing the processor architecture

        FullMask = 0x00F0,     // Bits describing the PA incl. Specified
        Shift = 0x0004,     // NOT A FLAG, shift count in PA flags <--> index conversion

        /// <summary>
        /// From "DebuggableAttribute".
        /// </summary>
        EnableJITcompileTracking = 0x8000, 
        /// <summary>
        /// From "DebuggableAttribute".
        /// </summary>
        DisableJITcompileOptimizer = 0x4000, 

        /// <summary>
        /// The assembly can be retargeted (at runtime) to an assembly from a different publisher.
        /// </summary>
        Retargetable = 0x0100,    
                                  
    }
}
