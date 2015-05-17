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
        public static readonly X86OpCode[] SingleByteOpCodes = new X86OpCode[0x100];

        // TODO: remove None operand values.
        // TODO: maybe merge byte+reg opcodes into one field?
        // TODO: opcode mnemonic aliases.

        #region 0x00 -> 0x0F

        public static readonly X86OpCode Add_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Add, 0x00,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Add_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Add, 0x01,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Add_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.Add, 0x02,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Add_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Add, 0x03,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Add_Al_Imm8 = new X86OpCode(X86Mnemonic.Add, 0x04,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Add_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Add, 0x05,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Push_Es = new X86OpCode(X86Mnemonic.Push, 0x06,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true);

        public static readonly X86OpCode Pop_Es = new X86OpCode(X86Mnemonic.Pop, 0x07,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true);

        public static readonly X86OpCode Or_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Or, 0x08,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Or_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Or, 0x09,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Or_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.Or, 0x0A,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Or_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Or, 0x0B,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Or_Al_Imm8 = new X86OpCode(X86Mnemonic.Or, 0x0C,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Or_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Or, 0x0D,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Push_Cs = new X86OpCode(X86Mnemonic.Push, 0x0E,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true);

        // TODO: 0x0F: Two byte opcodes prefix

        #endregion

        #region 0x10 -> 0x1F

        public static readonly X86OpCode Adc_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Adc, 0x10,
           ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
           ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Adc_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Adc, 0x11,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Adc_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.Adc, 0x12,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Adc_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Adc, 0x13,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Adc_Al_Imm8 = new X86OpCode(X86Mnemonic.Adc, 0x14,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Adc_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Adc, 0x15,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Push_Ss = new X86OpCode(X86Mnemonic.Push, 0x16,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true);

        public static readonly X86OpCode Pop_Ss = new X86OpCode(X86Mnemonic.Pop, 0x17,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true);

        public static readonly X86OpCode Sbb_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Sbb, 0x18,
           ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
           ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Sbb_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Sbb, 0x19,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Sbb_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.Sbb, 0x1A,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Sbb_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Sbb, 0x1B,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Sbb_Al_Imm8 = new X86OpCode(X86Mnemonic.Sbb, 0x1C,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Sbb_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Sbb, 0x1D,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Push_Ds = new X86OpCode(X86Mnemonic.Push, 0x1E,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true);

        public static readonly X86OpCode Pop_Ds = new X86OpCode(X86Mnemonic.Pop, 0x1F,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true);

        #endregion

        #region 0x20 -> 0x2F

        public static readonly X86OpCode And_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.And, 0x20,
           ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
           ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode And_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.And, 0x21,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode And_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.And, 0x22,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode And_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.And, 0x23,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode And_Al_Imm8 = new X86OpCode(X86Mnemonic.And, 0x24,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode And_Eax_Imm1632 = new X86OpCode(X86Mnemonic.And, 0x25,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        // TODO: 0x26: ES override

        public static readonly X86OpCode Daa = new X86OpCode(X86Mnemonic.Daa, 0x27,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Sub_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Sub, 0x28,
           ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
           ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Sub_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Sub, 0x29,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Sub_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.Sub, 0x2A,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Sub_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Sub, 0x2B,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Sub_Al_Imm8 = new X86OpCode(X86Mnemonic.Sub, 0x2C,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Sub_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Sub, 0x2D,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        // TODO: 0x2E: CS override

        public static readonly X86OpCode Das = new X86OpCode(X86Mnemonic.Das, 0x2F,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0x30 -> 0x3F

        public static readonly X86OpCode Xor_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Xor, 0x30,
           ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
           ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Xor_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Xor, 0x31,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Xor_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.Xor, 0x32,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Xor_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Xor, 0x33,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Xor_Al_Imm8 = new X86OpCode(X86Mnemonic.Xor, 0x34,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Xor_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Xor, 0x35,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        // TODO: 0x36: SS override

        public static readonly X86OpCode Aaa = new X86OpCode(X86Mnemonic.Aaa, 0x37,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Cmp_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Cmp, 0x38,
           ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
           ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Cmp_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Cmp, 0x39,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Cmp_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.Cmp, 0x3A,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Cmp_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Cmp, 0x3B,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Cmp_Al_Imm8 = new X86OpCode(X86Mnemonic.Cmp, 0x3C,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Cmp_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Cmp, 0x3D,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        // TODO: 0x3E: DS override

        public static readonly X86OpCode Aas = new X86OpCode(X86Mnemonic.Aas, 0x3F,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0x40 -> 0x4F

        public static readonly X86OpCode Inc_Eax = new X86OpCode(X86Mnemonic.Inc, 0x40,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Inc_Ecx = new X86OpCode(X86Mnemonic.Inc, 0x41,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Inc_Edx = new X86OpCode(X86Mnemonic.Inc, 0x42,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Inc_Ebx = new X86OpCode(X86Mnemonic.Inc, 0x43,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Inc_Esp = new X86OpCode(X86Mnemonic.Inc, 0x44,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Inc_Ebp = new X86OpCode(X86Mnemonic.Inc, 0x45,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Inc_Esi = new X86OpCode(X86Mnemonic.Inc, 0x46,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Inc_Edi = new X86OpCode(X86Mnemonic.Inc, 0x47,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Dec_Eax = new X86OpCode(X86Mnemonic.Dec, 0x48,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Dec_Ecx = new X86OpCode(X86Mnemonic.Dec, 0x49,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Dec_Edx = new X86OpCode(X86Mnemonic.Dec, 0x4A,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Dec_Ebx = new X86OpCode(X86Mnemonic.Dec, 0x4B,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Dec_Esp = new X86OpCode(X86Mnemonic.Dec, 0x4C,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Dec_Ebp = new X86OpCode(X86Mnemonic.Dec, 0x4D,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Dec_Esi = new X86OpCode(X86Mnemonic.Dec, 0x4E,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Dec_Edi = new X86OpCode(X86Mnemonic.Dec, 0x4F,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0x50 -> 0x5F

        public static readonly X86OpCode Push_Eax = new X86OpCode(X86Mnemonic.Push, 0x50,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Push_Ecx = new X86OpCode(X86Mnemonic.Push, 0x51,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Push_Edx = new X86OpCode(X86Mnemonic.Push, 0x52,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Push_Ebx = new X86OpCode(X86Mnemonic.Push, 0x53,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Push_Esp = new X86OpCode(X86Mnemonic.Push, 0x54,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Push_Ebp = new X86OpCode(X86Mnemonic.Push, 0x55,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Push_Esi = new X86OpCode(X86Mnemonic.Push, 0x56,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Push_Edi = new X86OpCode(X86Mnemonic.Push, 0x57,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pop_Eax = new X86OpCode(X86Mnemonic.Pop, 0x58,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pop_Ecx = new X86OpCode(X86Mnemonic.Pop, 0x59,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pop_Edx = new X86OpCode(X86Mnemonic.Pop, 0x5A,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pop_Ebx = new X86OpCode(X86Mnemonic.Pop, 0x5B,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pop_Esp = new X86OpCode(X86Mnemonic.Pop, 0x5C,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pop_Ebp = new X86OpCode(X86Mnemonic.Pop, 0x5D,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pop_Esi = new X86OpCode(X86Mnemonic.Pop, 0x5E,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pop_Edi = new X86OpCode(X86Mnemonic.Pop, 0x5F,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0x60 -> 0x6F
        
        public static readonly X86OpCode Pushad = new X86OpCode(X86Mnemonic.Pushad, 0x60,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Popad = new X86OpCode(X86Mnemonic.Popad, 0x61,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Bound = new X86OpCode(X86Mnemonic.Bound, 0x62,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Arpl = new X86OpCode(X86Mnemonic.Arpl, 0x63,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Word << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Word, true);

        // TODO: 0x64: FS override.
        // TODO: 0x65: GS override.
        // TODO: 0x66: Operand-size override.
        // TODO: 0x67: Precision-size override.

        public static readonly X86OpCode Push_Imm1632 = new X86OpCode(X86Mnemonic.Push, 0x68,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        // TODO: 0x69: IMul_Reg1632_RegOrMem1632_Imm1632 (registerToken = true)

        public static readonly X86OpCode Push_Imm8 = new X86OpCode(X86Mnemonic.Push, 0x6A,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        // TODO: 0x6B: IMul_Reg1632_RegOrMem1632_Imm8 (registerToken = true)

        public static readonly X86OpCode Ins_Mem8_Dx = new X86OpCode(X86Mnemonic.Ins, 0x6C,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Ins_Mem1632_Dx = new X86OpCode(X86Mnemonic.Ins, 0x6D,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Outs_Mem8_Dx = new X86OpCode(X86Mnemonic.Outs, 0x6E,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Outs_Mem1632_Dx = new X86OpCode(X86Mnemonic.Outs, 0x6F,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0x70 -> 0x7F

        public static readonly X86OpCode Jo_Rel8 = new X86OpCode(X86Mnemonic.Jo, 0x70,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jno_Rel8 = new X86OpCode(X86Mnemonic.Jno, 0x71,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jb_Rel8 = new X86OpCode(X86Mnemonic.Jb, 0x72,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jnb_Rel8 = new X86OpCode(X86Mnemonic.Jnb, 0x73,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Je_Rel8 = new X86OpCode(X86Mnemonic.Je, 0x74,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jne_Rel8 = new X86OpCode(X86Mnemonic.Jne, 0x75,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jbe_Rel8 = new X86OpCode(X86Mnemonic.Jbe, 0x76,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Ja_Rel8 = new X86OpCode(X86Mnemonic.Ja, 0x77,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Js_Rel8 = new X86OpCode(X86Mnemonic.Js, 0x78,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jns_Rel8 = new X86OpCode(X86Mnemonic.Jns, 0x79,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jpe_Rel8 = new X86OpCode(X86Mnemonic.Jpe, 0x7A,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jpo_Rel8 = new X86OpCode(X86Mnemonic.Jpo, 0x7B,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jl_Rel8 = new X86OpCode(X86Mnemonic.Jl, 0x7C,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jge_Rel8 = new X86OpCode(X86Mnemonic.Jge, 0x7D,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jle_Rel8 = new X86OpCode(X86Mnemonic.Jle, 0x7E,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jg_Rel8 = new X86OpCode(X86Mnemonic.Jg, 0x7F,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0x80 -> 0x8F

        public static readonly X86OpCode Arithmetic_RegOrMem8_Imm8 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Add, X86Mnemonic.Or, X86Mnemonic.Adc, X86Mnemonic.Sbb, 
            X86Mnemonic.And, X86Mnemonic.Sub, X86Mnemonic.Xor, X86Mnemonic.Cmp
        }, 0x00008000,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, true, true);

        public static readonly X86OpCode Arithmetic_RegOrMem32_Imm1632 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Add, X86Mnemonic.Or, X86Mnemonic.Adc, X86Mnemonic.Sbb, 
            X86Mnemonic.And, X86Mnemonic.Sub, X86Mnemonic.Xor, X86Mnemonic.Cmp
        }, 0x00008100,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Dword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, true, true);


        public static readonly X86OpCode Arithmetic_RegOrMem8_Imm8_2 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Add, X86Mnemonic.Or, X86Mnemonic.Adc, X86Mnemonic.Sbb, 
            X86Mnemonic.And, X86Mnemonic.Sub, X86Mnemonic.Xor, X86Mnemonic.Cmp
        }, 0x00008200,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, true, true);

        public static readonly X86OpCode Arithmetic_RegOrMem32_Imm8 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Add, X86Mnemonic.Or, X86Mnemonic.Adc, X86Mnemonic.Sbb, 
            X86Mnemonic.And, X86Mnemonic.Sub, X86Mnemonic.Xor, X86Mnemonic.Cmp
        }, 0x00008300,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Dword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, true, true);

        public static readonly X86OpCode Test_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Test, 0x84,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Test_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Test, 0x85,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Xchg_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Xchg, 0x86,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Xchg_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Xchg, 0x87,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Mov_RegOrMem8_Reg8 = new X86OpCode(X86Mnemonic.Mov, 0x88,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Mov_RegOrMem1632_Reg1632 = new X86OpCode(X86Mnemonic.Mov, 0x89,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.Register << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Mov_Reg8_RegOrMem8 = new X86OpCode(X86Mnemonic.Mov, 0x8A,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Byte, true);

        public static readonly X86OpCode Mov_Reg1632_RegOrMem1632 = new X86OpCode(X86Mnemonic.Mov, 0x8B,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, true);

        public static readonly X86OpCode Mov_RegOrMem16_SReg = new X86OpCode(X86Mnemonic.Mov, 0x8C,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Word << 0x10) |
            ((byte)X86OperandType.SegmentRegister << 0x08) | (byte)X86OperandSize.Word, true);

        public static readonly X86OpCode Lea_RegOrMem1632_Mem32 = new X86OpCode(X86Mnemonic.Lea, 0x8D,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Dword, true);

        public static readonly X86OpCode Mov_SReg_RegOrMem16 = new X86OpCode(X86Mnemonic.Mov, 0x8E,
            ((byte)X86OperandType.SegmentRegister << 0x18) | ((byte)X86OperandSize.Word << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.Word, true);

        public static readonly X86OpCode Pop_RegOrMem1632 = new X86OpCode(X86Mnemonic.Pop, 0x8F,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true);

        #endregion

        #region 0x90 -> 0x9F

        public static readonly X86OpCode Nop = new X86OpCode(X86Mnemonic.Nop, 0x90,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Xchg_Ecx_Eax = new X86OpCode(X86Mnemonic.Xchg, 0x91,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Xchg_Edx_Eax = new X86OpCode(X86Mnemonic.Xchg, 0x92,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Xchg_Ebx_Eax = new X86OpCode(X86Mnemonic.Xchg, 0x93,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Xchg_Esp_Eax = new X86OpCode(X86Mnemonic.Xchg, 0x94,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Xchg_Ebp_Eax = new X86OpCode(X86Mnemonic.Xchg, 0x95,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Xchg_Esi_Eax = new X86OpCode(X86Mnemonic.Xchg, 0x96,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Xchg_Edi_Eax = new X86OpCode(X86Mnemonic.Xchg, 0x97,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        // TODO: 0xF3 0x90 pause

        public static readonly X86OpCode Cwde = new X86OpCode(X86Mnemonic.Cwde, 0x98,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Cdq = new X86OpCode(X86Mnemonic.Cdq, 0x99,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Call_Far_Rel48 = new X86OpCode(X86Mnemonic.Call_Far, 0x9A,
            ((byte)X86OperandType.DirectAddress << 0x18) | ((byte)X86OperandSize.Fword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Wait = new X86OpCode(X86Mnemonic.Wait, 0x9B,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Pushfd = new X86OpCode(X86Mnemonic.Pushfd, 0x9C,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Popfd = new X86OpCode(X86Mnemonic.Popfd, 0x9D,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Sahf = new X86OpCode(X86Mnemonic.Sahf, 0x9E,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Lahf = new X86OpCode(X86Mnemonic.Lahf, 0x9F,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0xA0 -> 0xAF

        public static readonly X86OpCode Mov_Al_Mem8 = new X86OpCode(X86Mnemonic.Mov, 0xA0,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.MemoryAddress << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Eax_Mem1632 = new X86OpCode(X86Mnemonic.Mov, 0xA1,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.MemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_Mem8_Al = new X86OpCode(X86Mnemonic.Mov, 0xA2,
            ((byte)X86OperandType.MemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterAl << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Mem1632_Eax = new X86OpCode(X86Mnemonic.Mov, 0xA3,
            ((byte)X86OperandType.MemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Movsb = new X86OpCode(X86Mnemonic.Movsb, 0xA4,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Movsd = new X86OpCode(X86Mnemonic.Movsd, 0xA5,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Cmpsb = new X86OpCode(X86Mnemonic.Cmpsb, 0xA6,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Cmpsd = new X86OpCode(X86Mnemonic.Cmpsd, 0xA7,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Test_Al_Imm8 = new X86OpCode(X86Mnemonic.Test, 0xA8,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Test_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Test, 0xA9,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Stosb = new X86OpCode(X86Mnemonic.Stosb, 0xAA,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Stosd = new X86OpCode(X86Mnemonic.Stosd, 0xAB,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Lodsb = new X86OpCode(X86Mnemonic.Lodsb, 0xAC,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Lodsd = new X86OpCode(X86Mnemonic.Lodsd, 0xAD,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Scasb = new X86OpCode(X86Mnemonic.Scasb, 0xAE,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Scasd = new X86OpCode(X86Mnemonic.Scasd, 0xAF,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0xB0 -> 0xBF

        public static readonly X86OpCode Mov_Al_Imm8 = new X86OpCode(X86Mnemonic.Mov, 0xB0,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Cl_Imm8 = new X86OpCode(X86Mnemonic.Mov, 0xB1,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Dl_Imm8 = new X86OpCode(X86Mnemonic.Mov, 0xB2,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Bl_Imm8 = new X86OpCode(X86Mnemonic.Mov, 0xB3,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Ah_Imm8 = new X86OpCode(X86Mnemonic.Mov, 0xB4,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Ch_Imm8 = new X86OpCode(X86Mnemonic.Mov, 0xB5,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Dh_Imm8 = new X86OpCode(X86Mnemonic.Mov, 0xB6,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Bh_Imm8 = new X86OpCode(X86Mnemonic.Mov, 0xB7,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Mov_Eax_Imm1632 = new X86OpCode(X86Mnemonic.Mov, 0xB8,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_Ecx_Imm1632 = new X86OpCode(X86Mnemonic.Mov, 0xB9,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_Edx_Imm1632 = new X86OpCode(X86Mnemonic.Mov, 0xBA,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_Ebx_Imm1632 = new X86OpCode(X86Mnemonic.Mov, 0xBB,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_Esp_Imm1632 = new X86OpCode(X86Mnemonic.Mov, 0xBC,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_Ebp_Imm1632 = new X86OpCode(X86Mnemonic.Mov, 0xBD,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_Esi_Imm1632 = new X86OpCode(X86Mnemonic.Mov, 0xBE,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_Edi_Imm1632 = new X86OpCode(X86Mnemonic.Mov, 0xBF,
            ((byte)X86OperandType.OpCodeRegister << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        #endregion

        #region 0xC0 -> 0xCF

        public static readonly X86OpCode BitShift_RegOrMem8_Imm8 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Rol, X86Mnemonic.Ror, X86Mnemonic.Rcl, X86Mnemonic.Rcr,
            X86Mnemonic.Shl, X86Mnemonic.Shr, X86Mnemonic.Sal, X86Mnemonic.Sar
        }, 0x0000C000,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, true, true);

        public static readonly X86OpCode BitShift_RegOrMem1632_Imm8 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Rol, X86Mnemonic.Ror, X86Mnemonic.Rcl, X86Mnemonic.Rcr,
            X86Mnemonic.Shl, X86Mnemonic.Shr, X86Mnemonic.Sal, X86Mnemonic.Sar
        }, 0x0000C100,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, true, true);

        public static readonly X86OpCode Retn_Imm16 = new X86OpCode(X86Mnemonic.Retn, 0xC2,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Word << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Retn = new X86OpCode(X86Mnemonic.Retn, 0xC3,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Les_Reg1632_MemOrReg1632 = new X86OpCode(X86Mnemonic.Les, 0xC4,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Lds_Reg1632_MemOrReg1632 = new X86OpCode(X86Mnemonic.Lds, 0xC5,
            ((byte)X86OperandType.Register << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x08) | (byte)X86OperandSize.WordOrDword, false);

        public static readonly X86OpCode Mov_RegOrMem8_Imm8 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Mov
        }, 0x0000C600,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, true, true);

        public static readonly X86OpCode Mov_RegOrMem1632_Imm1632 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Mov
        }, 0x0000C700,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.WordOrDword, true, true);

        public static readonly X86OpCode Enter_Imm16_Imm8 = new X86OpCode(X86Mnemonic.Enter, 0xC8,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Word << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Leave = new X86OpCode(X86Mnemonic.Leave, 0xC9,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Retf_Imm16 = new X86OpCode(X86Mnemonic.Retf, 0xCA,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Word << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Retf = new X86OpCode(X86Mnemonic.Retf, 0xCB,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Int3 = new X86OpCode(X86Mnemonic.Int3, 0xCC,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Int_Imm8 = new X86OpCode(X86Mnemonic.Int, 0xCD,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Into = new X86OpCode(X86Mnemonic.Into, 0xCE,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Iretd = new X86OpCode(X86Mnemonic.Iretd, 0xCF,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        #endregion

        #region 0xD0 -> 0xDF

        public static readonly X86OpCode BitShift_RegOrMem8_1 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Rol, X86Mnemonic.Ror, X86Mnemonic.Rcl, X86Mnemonic.Rcr,
            X86Mnemonic.Shl, X86Mnemonic.Shr, X86Mnemonic.Sal, X86Mnemonic.Sar
        }, 0x0000D000,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateOne << 0x08) | (byte)X86OperandSize.None, true, true);

        public static readonly X86OpCode BitShift_RegOrMem1632_1 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Rol, X86Mnemonic.Ror, X86Mnemonic.Rcl, X86Mnemonic.Rcr,
            X86Mnemonic.Shl, X86Mnemonic.Shr, X86Mnemonic.Sal, X86Mnemonic.Sar
        }, 0x0000D100,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.ImmediateOne << 0x08) | (byte)X86OperandSize.None, true, true);

        public static readonly X86OpCode BitShift_RegOrMem8_Cl = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Rol, X86Mnemonic.Ror, X86Mnemonic.Rcl, X86Mnemonic.Rcr,
            X86Mnemonic.Shl, X86Mnemonic.Shr, X86Mnemonic.Sal, X86Mnemonic.Sar
        }, 0x0000D200,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterCl << 0x08) | (byte)X86OperandSize.Byte, true, true);

        public static readonly X86OpCode BitShift_RegOrMem1632_Cl = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Rol, X86Mnemonic.Ror, X86Mnemonic.Rcl, X86Mnemonic.Rcr,
            X86Mnemonic.Shl, X86Mnemonic.Shr, X86Mnemonic.Sal, X86Mnemonic.Sar
        }, 0x0000D300,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.WordOrDword << 0x10) |
            ((byte)X86OperandType.RegisterCl << 0x08) | (byte)X86OperandSize.Byte, true, true);

        public static readonly X86OpCode Amx_Imm8 = new X86OpCode(X86Mnemonic.Amx, 0xD4,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Adx_Imm8 = new X86OpCode(X86Mnemonic.Adx, 0xD5,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Salc = new X86OpCode(X86Mnemonic.Salc, 0xD6,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Xlatb = new X86OpCode(X86Mnemonic.Xlatb, 0xD7,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        // TODO: 0xD8 -> 0xDF

        #endregion

        #region 0xE0 -> 0xEF

        public static readonly X86OpCode Loopne_Rel8 = new X86OpCode(X86Mnemonic.Loopne, 0xE0,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Loope_Rel8 = new X86OpCode(X86Mnemonic.Loope, 0xE1,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Loop_Rel8 = new X86OpCode(X86Mnemonic.Loop, 0xE2,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jecxz_Rel8 = new X86OpCode(X86Mnemonic.Jecxz, 0xE3,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode In_Al_Imm8 = new X86OpCode(X86Mnemonic.In, 0xE4,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode In_Eax_Imm8 = new X86OpCode(X86Mnemonic.In, 0xE5,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.Dword << 0x10) |
            ((byte)X86OperandType.ImmediateData << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Out_Imm8_Al = new X86OpCode(X86Mnemonic.Out, 0xE6,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterAl << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Out_Imm8_Eax = new X86OpCode(X86Mnemonic.Out, 0xE7,
            ((byte)X86OperandType.ImmediateData << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.Dword, false);

        public static readonly X86OpCode Call_Rel1632 = new X86OpCode(X86Mnemonic.Call, 0xE8,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Dword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jmp_Rel1632 = new X86OpCode(X86Mnemonic.Jmp, 0xE9,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Dword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jmp_Far_Rel48 = new X86OpCode(X86Mnemonic.Jmp_Far, 0xEA,
            ((byte)X86OperandType.DirectAddress << 0x18) | ((byte)X86OperandSize.Fword << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Jmp_Rel8 = new X86OpCode(X86Mnemonic.Jmp, 0xEB,
            ((byte)X86OperandType.RelativeOffset << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode In_Al_Dx = new X86OpCode(X86Mnemonic.In, 0xEC,
            ((byte)X86OperandType.RegisterAl << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.RegisterDx << 0x08) | (byte)X86OperandSize.Word, false);

        public static readonly X86OpCode In_Eax_Dx = new X86OpCode(X86Mnemonic.In, 0xED,
            ((byte)X86OperandType.RegisterEax << 0x18) | ((byte)X86OperandSize.Dword << 0x10) |
            ((byte)X86OperandType.RegisterDx << 0x08) | (byte)X86OperandSize.Word, false);

        public static readonly X86OpCode Out_Dx_Al = new X86OpCode(X86Mnemonic.Out, 0xEE,
            ((byte)X86OperandType.RegisterDx << 0x18) | ((byte)X86OperandSize.Word << 0x10) |
            ((byte)X86OperandType.RegisterAl << 0x08) | (byte)X86OperandSize.Byte, false);

        public static readonly X86OpCode Out_Dx_Eax = new X86OpCode(X86Mnemonic.Out, 0xEF,
            ((byte)X86OperandType.RegisterDx << 0x18) | ((byte)X86OperandSize.Word << 0x10) |
            ((byte)X86OperandType.RegisterEax << 0x08) | (byte)X86OperandSize.Dword, false);

        
        #endregion

        #region 0xF0 -> 0xFF

        // TODO: make prefix.
        public static readonly X86OpCode Lock = new X86OpCode(X86Mnemonic.Lock, 0xF0,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Int1 = new X86OpCode(X86Mnemonic.Int1, 0xF1,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        // TODO: 0xF2 Scalar double-precision prefix.

        public static readonly X86OpCode Rep = new X86OpCode(X86Mnemonic.Rep, 0xF3,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Hlt = new X86OpCode(X86Mnemonic.Hlt, 0xF4,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Cmc = new X86OpCode(X86Mnemonic.Cmc, 0xF5,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode F6 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Test, X86Mnemonic.Test, X86Mnemonic.Not, X86Mnemonic.Neg,
            X86Mnemonic.Mul, X86Mnemonic.Imul, X86Mnemonic.Div, X86Mnemonic.Idiv
        }, 0x0000F600,
            new X86OperandType[]
            {
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
            },
            new X86OperandSize[]
            {
                X86OperandSize.Byte,
                X86OperandSize.Byte,
                X86OperandSize.Byte,
                X86OperandSize.Byte,
                X86OperandSize.Byte,
                X86OperandSize.Byte,
                X86OperandSize.Byte,
                X86OperandSize.Byte,
            },
            new X86OperandType[]
            {
                X86OperandType.ImmediateData,
                X86OperandType.ImmediateData,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
            },
            new X86OperandSize[]
            {
                X86OperandSize.Byte,
                X86OperandSize.Byte,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
            });

        public static readonly X86OpCode F7 = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Test, X86Mnemonic.Test, X86Mnemonic.Not, X86Mnemonic.Neg,
            X86Mnemonic.Mul, X86Mnemonic.Imul, X86Mnemonic.Div, X86Mnemonic.Idiv
        }, 0x0000F700,
            new X86OperandType[]
            {
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
            },
            new X86OperandSize[]
            {
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
            },
            new X86OperandType[]
            {
                X86OperandType.ImmediateData,
                X86OperandType.ImmediateData,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
            },
            new X86OperandSize[]
            {
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
            });

        public static readonly X86OpCode Clc = new X86OpCode(X86Mnemonic.Clc, 0xF8,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Stc = new X86OpCode(X86Mnemonic.Stc, 0xF9,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Cli = new X86OpCode(X86Mnemonic.Cli, 0xFA,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Sti = new X86OpCode(X86Mnemonic.Sti, 0xFB,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Cld = new X86OpCode(X86Mnemonic.Cld, 0xFC,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode Std = new X86OpCode(X86Mnemonic.Std, 0xFD,
            ((byte)X86OperandType.None << 0x18) | ((byte)X86OperandSize.None << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, false);

        public static readonly X86OpCode FE = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Inc, X86Mnemonic.Dec, 
        }, 0x0000FE00,
            ((byte)X86OperandType.RegisterOrMemoryAddress << 0x18) | ((byte)X86OperandSize.Byte << 0x10) |
            ((byte)X86OperandType.None << 0x08) | (byte)X86OperandSize.None, true, true);

        public static readonly X86OpCode FF = new X86OpCode(new X86Mnemonic[]
        {
            X86Mnemonic.Inc, X86Mnemonic.Dec, X86Mnemonic.Call, X86Mnemonic.Call_Far,
            X86Mnemonic.Jmp, X86Mnemonic.Jmp_Far, X86Mnemonic.Push,
        }, 0x0000FF00,
            new X86OperandType[]
            {
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
                X86OperandType.RegisterOrMemoryAddress,
            },
            new X86OperandSize[]
            {
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.WordOrDword,
                X86OperandSize.Fword,
                X86OperandSize.WordOrDword,
                X86OperandSize.Fword,
                X86OperandSize.WordOrDword,
            },
            new X86OperandType[]
            {
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
                X86OperandType.None,
            },
            new X86OperandSize[]
            {
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
                X86OperandSize.None,
            });

        #endregion

    }
    // ReSharper restore InconsistentNaming
}
