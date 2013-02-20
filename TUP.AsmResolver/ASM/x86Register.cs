using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// Describes the register(s) an <see cref="TUP.AsmResolver.ASM.x86OpCode"/> use.
    /// </summary>
    public enum x86Register : byte
    {

        /// <summary>
        /// Accumulator 32-bit Register.
        /// </summary>
        EAX = 0x0,
        /// <summary>
        /// Base 32-bit Register.
        /// </summary>
        EBX = 0x3,
        /// <summary>
        /// Counter 32-bit Register.
        /// </summary>
        ECX = 0x1,
        /// <summary>
        /// Data 32-bit Register.
        /// </summary>
        EDX = 0x2,
        /// <summary>
        /// Stack pointer 32-bit Register.
        /// </summary>
        ESP = 0x4,
        /// <summary>
        /// Stack Base pointer 32-bit Register.
        /// </summary>
        EBP = 0x5,
        /// <summary>
        /// Source index 32-bit Register.
        /// </summary>
        ESI = 0x6,
        /// <summary>
        /// Destination pointer 32-bit Register. 
        /// </summary>
        EDI = 0x7,


        Bit16Mask = 0x10,

        /// <summary>
        /// Accumulator 16-bit Register.
        /// </summary>
        AX = EAX | Bit16Mask,
        /// <summary>
        /// Base 16-bit Register.
        /// </summary>
        BX = EBX | Bit16Mask,
        /// <summary>
        /// Counter 16-bit Register.
        /// </summary>
        CX = ECX | Bit16Mask,
        /// <summary>
        /// Data 16-bit Register.
        /// </summary>
        DX = EDX | Bit16Mask,


        Bit8Mask = 0x20,

        /// <summary>
        /// Accumulator low 8-bit Register.
        /// </summary>
        AL = EAX | Bit8Mask,
        /// <summary>
        /// Base low 8-bit Register.
        /// </summary>
        BL = EBX | Bit8Mask,
        /// <summary>
        /// Counter low 8-bit Register.
        /// </summary>
        CL = ECX | Bit8Mask,
        /// <summary>
        /// Data low 8-bit Register.
        /// </summary>
        DL = EDX | Bit8Mask,

        /// <summary>
        /// Accumulator high 8-bit Register.
        /// </summary>
        AH = AL + 0x4,
        /// <summary>
        /// Base high 8-bit Register.
        /// </summary>
        BH = BL + 0x4,
        /// <summary>
        /// Counter high 8-bit Register.
        /// </summary>
        CH = CL + 0x4,
        /// <summary>
        /// Data high 8-bit Register.
        /// </summary>
        DH = DL + 0x4,
    }
}
