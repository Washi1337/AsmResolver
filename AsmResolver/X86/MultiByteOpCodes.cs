using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Reference: http://ref.x86asm.net/coder32.html

namespace AsmResolver.X86
{
    // ReSharper disable InconsistentNaming
    public static partial class X86OpCodes
    {
        // TODO: Change to arrays.
        public static readonly X86OpCode[] MultiByteOpCodes = new X86OpCode[0x100];

        #region 0x80 -> 0x8F

        public static readonly X86OpCode Jo_Rel1632 = new X86OpCode(X86Mnemonic.Jo, 0x000F8000,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jno_Rel1632 = new X86OpCode(X86Mnemonic.Jno, 0x000F8100,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jb_Rel1632 = new X86OpCode(X86Mnemonic.Jb, 0x000F8200,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jnb_Rel1632 = new X86OpCode(X86Mnemonic.Jnb, 0x000F8300,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Je_Rel1632 = new X86OpCode(X86Mnemonic.Je, 0x000F8400,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jne_Rel1632 = new X86OpCode(X86Mnemonic.Jne, 0x000F8500,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jbe_Rel1632 = new X86OpCode(X86Mnemonic.Jbe, 0x000F8600,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Ja_Rel1632 = new X86OpCode(X86Mnemonic.Ja, 0x000F8700,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Js_Rel1632 = new X86OpCode(X86Mnemonic.Js, 0x000F8800,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jns_Rel1632 = new X86OpCode(X86Mnemonic.Jns, 0x000F8900,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jpe_Rel1632 = new X86OpCode(X86Mnemonic.Jpe, 0x000F8A00,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jpo_Rel1632 = new X86OpCode(X86Mnemonic.Jpo, 0x000F8B00,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jl_Rel1632 = new X86OpCode(X86Mnemonic.Jl, 0x000F8C00,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jge_Rel1632 = new X86OpCode(X86Mnemonic.Jge, 0x000F8D00,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jle_Rel1632 = new X86OpCode(X86Mnemonic.Jle, 0x000F8E00,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jg_Rel1632 = new X86OpCode(X86Mnemonic.Jg, 0x000F8F00,
            ((byte)X86AddressingMethod.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86AddressingMethod.None << 0x08) | (byte)X86OperandSize.None, false);
        #endregion

    }
    // ReSharper restore InconsistentNaming
}
