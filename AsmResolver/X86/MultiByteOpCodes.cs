

// Reference: http://ref.x86asm.net/coder32.html

namespace AsmResolver.X86
{
    // ReSharper disable InconsistentNaming
    public static partial class X86OpCodes
    {
        public static readonly X86OpCode[] MultiByteOpCodes = new X86OpCode[0x100];

        #region 0x80 -> 0x8F

        public static readonly X86OpCode Jo_Rel1632 = new X86OpCode(X86Mnemonic.Jo, 0x000F8000,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jno_Rel1632 = new X86OpCode(X86Mnemonic.Jno, 0x000F8100,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jb_Rel1632 = new X86OpCode(X86Mnemonic.Jb, 0x000F8200,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jnb_Rel1632 = new X86OpCode(X86Mnemonic.Jnb, 0x000F8300,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Je_Rel1632 = new X86OpCode(X86Mnemonic.Je, 0x000F8400,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jne_Rel1632 = new X86OpCode(X86Mnemonic.Jne, 0x000F8500,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jbe_Rel1632 = new X86OpCode(X86Mnemonic.Jbe, 0x000F8600,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Ja_Rel1632 = new X86OpCode(X86Mnemonic.Ja, 0x000F8700,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Js_Rel1632 = new X86OpCode(X86Mnemonic.Js, 0x000F8800,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jns_Rel1632 = new X86OpCode(X86Mnemonic.Jns, 0x000F8900,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jpe_Rel1632 = new X86OpCode(X86Mnemonic.Jpe, 0x000F8A00,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jpo_Rel1632 = new X86OpCode(X86Mnemonic.Jpo, 0x000F8B00,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jl_Rel1632 = new X86OpCode(X86Mnemonic.Jl, 0x000F8C00,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jge_Rel1632 = new X86OpCode(X86Mnemonic.Jge, 0x000F8D00,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jle_Rel1632 = new X86OpCode(X86Mnemonic.Jle, 0x000F8E00,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jg_Rel1632 = new X86OpCode(X86Mnemonic.Jg, 0x000F8F00,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);
        #endregion

        #region 0xB0 -> 0xBF
        
        public static readonly X86OpCode Cmpxchg_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Cmpxchg, 0x000FB000,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);
        
        public static readonly X86OpCode Cmpxchg_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Cmpxchg, 0x000FB100,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);
        
        // LSS
        
        public static readonly X86OpCode Btr_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Cmpxchg, 0x000FB400,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);
        
        // LFS
        // LGS
        
        public static readonly X86OpCode Movzx_Reg1632_RegOrMem8 = new X86OpCode(X86Mnemonic.Movzx, 0x000FB600,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);
        
        public static readonly X86OpCode Movzx_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Movzx, 0x000FB700,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);
        
        // POPCNT
        
        // BT / BTS / BTR BTC 
        
        public static readonly X86OpCode Btc_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Btc, 0x000FBB00,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);
        
        public static readonly X86OpCode Bsf_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Bsf, 0x000FBC00,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);
        
        public static readonly X86OpCode Bsr_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Bsr, 0x000FBD00,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);
        
        public static readonly X86OpCode Movsx_Reg1632_RegOrMem8 = new X86OpCode(X86Mnemonic.Movsx, 0x000FBE00,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);
        
        public static readonly X86OpCode Movsx_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Movsx, 0x000FBF00,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);
        
        
        #endregion
    }
    // ReSharper restore InconsistentNaming
}
