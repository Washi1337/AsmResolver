using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents the type of the CPU of a portable executable file.
    /// </summary>
    public enum Machine
    {
        /// <summary>
        /// The image is assumed to be applicable to any machine type.
        /// </summary>
        AnyCPU = 0,
        /// <summary>
        /// The image is asseumed to be applicable to an Intel 860 machine or later.
        /// </summary>
        I860 = 0x14d,
        /// <summary>
        /// The image is asseumed to be applicable to an Intel 386 machine or later and compatible processors.
        /// </summary>
        I386 = 0x14c,
        /// <summary>
        /// The image is asseumed to be applicable to a MIPS little-endian, 0540 big-endian machine.
        /// </summary>
        R3000 = 0x162,
        /// <summary>
        /// The image is asseumed to be applicable to a MIPS little endian machine.
        /// </summary>
        R4000 = 0x166,
        /// <summary>
        /// The image is asseumed to be applicable to a MIPS little-endian machine.
        /// </summary>
        R10000 = 0x168,
        /// <summary>
        /// The image is asseumed to be applicable to a MIPS little-endian WCE v2 machine.
        /// </summary>
        WCEMIPSV2 = 0x169,
        /// <summary>
        /// The image is asseumed to be applicable to an Alpha AXP machine.
        /// </summary>
        ALPHA = 0x184,
        /// <summary>
        /// The image is asseumed to be applicable to a Hitachi SH3 little-endian machine.
        /// </summary>
        SH3 = 0x1a2,
        /// <summary>
        /// The image is asseumed to be applicable to a Hitachi SH3DSP machine.
        /// </summary>
        SH3DSP = 0x1a3,
        /// <summary>
        /// The image is asseumed to be applicable to a Hitachi SH3E little-endian machine.
        /// </summary>
        SH3E = 0x1a4,
        /// <summary>
        /// The image is asseumed to be applicable to a Hitachi SH4 little-endian machine
        /// </summary>
        SH4 = 0x1a6,
        /// <summary>
        /// The image is asseumed to be applicable to a Hitachi SH5 machine
        /// </summary>
        SH5 = 0x1a8,
        /// <summary>
        /// The image is asseumed to be applicable to an ARM little endian machine.
        /// </summary>
        ARM = 0x1c0,
        /// <summary>
        /// No information is available about this machine yet.
        /// </summary>
        THUMB = 0x1c2,
        /// <summary>
        /// The image is asseumed to be applicable to an ARM v7 little endian machine.
        /// </summary>
        ARMV7 = 0x1c2,
        /// <summary>
        /// The image is asseumed to be applicable to a Matsushita AM33 machine.
        /// </summary>
        AM33 = 0x1d3,
        /// <summary>
        /// The image is asseumed to be applicable to a Power PC, little endian machine.
        /// </summary>
        POWERPC = 0x1f0,
        /// <summary>
        /// The image is asseumed to be applicable to a Power PC FP, little endian machine.
        /// </summary>
        POWERPCFP = 0x1f1,
        /// <summary>
        /// The image is asseumed to be applicable to a Power PC, big endian machine.
        /// </summary>
        POWERPCBE,
        /// <summary>
        /// The image is asseumed to be applicable to an Intel IA64 machine.
        /// </summary>
        IA64 = 0x200,
        /// <summary>
        /// The image is asseumed to be applicable to a MIPS16  machine.
        /// </summary>
        MIPS16 = 0x266,
        /// <summary>
        /// The image is asseumed to be applicable to a Alpha AXP 64-bit machine.
        /// </summary>
        ALPHA64 = 0x284,
        /// <summary>
        /// The image is asseumed to be applicable to a MIPS with FPU machine.
        /// </summary>
        MIPSFPU = 0x366,
        /// <summary>
        /// The image is asseumed to be applicable to a MIPS16 with FPU machine.
        /// </summary>
        MIPSFPU16 = 0x466,
        /// <summary>
        /// The image is asseumed to be applicable to a TRICORE machine.
        /// </summary>
        TRICORE = 0x520,
        /// <summary>
        /// No information is available about this machine yet.
        /// </summary>
        CEF = 0xcef,
        /// <summary>
        /// The image is asseumed to be applicable to a machine that supports EFI byte code.
        /// </summary>
        EBC = 0xebc,
        /// <summary>
        /// The image is asseumed to be applicable to a 64 bit machine
        /// </summary>
        AMD64 = 0x8664,
        /// <summary>
        /// The image is asseumed to be applicable to a Mitsubishi M32R little endian machine.
        /// </summary>
        M32R = 0x9041,
        /// <summary>
        /// No information is available about this machine yet.
        /// </summary>
        CEE = 0xc0ee,
        /// <summary>
        /// The image is asseumed to be applicable to a SPARC machine.
        /// </summary>
        SPARC = 0x2000,
        /// <summary>
        /// The image is asseumed to be applicable to a DEC Alpha AXP machine.
        /// </summary>
        DEC_Alpha_AXP = 0x183,
        /// <summary>
        /// The image is asseumed to be applicable to a Motorola 68000 series machine.
        /// </summary>
        M68K = 0x268,
        /// <summary>
        /// The image is asseumed to be applicable to an Intel EM machine.
        /// </summary>
        TAHOE = 0x7cc,

    }
}
