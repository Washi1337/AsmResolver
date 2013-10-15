using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable 1591

namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// Provides a list of field representations of the 32-bit assembly opcodes for reading and writing instructions. NOTE: NOT ALL INSTRUCTIONS ARE ADDED YET.
    /// </summary>
    public class x86OpCodes
    {
        //public static readonly byte[] GroupOpCodes = new byte[] {0x80, 0x81, 0x82, 0x83, 0xC0, 0xC1, 0xD0, 0xD1, 0xD2, 0xD3, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF, 0xF6, 0xF7, 0xFE, 0xFF};


        public static readonly x86OpCode Unknown                   = new x86OpCode("DB", null, 0, x86OperandType.None);

        public static readonly x86OpCode Add_r32Ptr_r8             = new x86OpCode("ADD", new byte[] { 0x0, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Add_r32Ptr_r32            = new x86OpCode("ADD", new byte[] { 0x1, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
        public static readonly x86OpCode Add_r8_r32Ptr             = new x86OpCode("ADD", new byte[] { 0x2, 0x0 }, 0, x86OperandType.Multiple32Or8Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Add_r32_r32Ptr            = new x86OpCode("ADD", new byte[] { 0x3, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Add_Al_Byte               = new x86OpCode("ADD AL,", new byte[] { 0x4 }, 1, x86OperandType.Byte);
        public static readonly x86OpCode Add_Eax_Dword             = new x86OpCode("ADD EAX,", new byte[] { 0x5 }, 4, x86OperandType.Dword);

        public static readonly x86OpCode Push_Es                   = new x86OpCode("PUSH ES", new byte[] { 0x6 }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Es                    = new x86OpCode("POP ES", new byte[] { 0x7 }, 0, x86OperandType.None);

        public static readonly x86OpCode Or_r32Ptr_r8              = new x86OpCode("OR", new byte[] { 0x8, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Or_r32Ptr_r32             = new x86OpCode("OR", new byte[] { 0x9, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
        public static readonly x86OpCode Or_r8_r32Ptr              = new x86OpCode("OR", new byte[] { 0xA, 0x0 }, 0, x86OperandType.Multiple32Or8Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Or_r32_r32Ptr             = new x86OpCode("OR", new byte[] { 0xB, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Or_Al_Byte                = new x86OpCode("OR AL,", new byte[] { 0xC }, 1, x86OperandType.Byte);
        public static readonly x86OpCode Or_Eax_Dword              = new x86OpCode("OR EAX,", new byte[] { 0xD }, 4, x86OperandType.Dword);

        public static readonly x86OpCode Push_Cs                   = new x86OpCode("PUSH CS", new byte[] { 0xE }, 0, x86OperandType.None);
      
        /*0x0F, multi byte opcodes */

        public static readonly x86OpCode Adc_r32Ptr_r8             = new x86OpCode("ADC", new byte[] { 0x10, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Adc_r32Ptr_r32            = new x86OpCode("ADC", new byte[] { 0x11, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
        public static readonly x86OpCode Adc_r8_r32Ptr             = new x86OpCode("ADC", new byte[] { 0x12, 0x0 }, 0, x86OperandType.Multiple32Or8Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Adc_r32_r32Ptr            = new x86OpCode("ADC", new byte[] { 0x13, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Adc_Al_Byte               = new x86OpCode("ADC AL,", new byte[] { 0x14 }, 1, x86OperandType.Byte);
        public static readonly x86OpCode Adc_Eax_Dword             = new x86OpCode("ADC EAX,", new byte[] { 0x15 }, 4, x86OperandType.Dword);

        public static readonly x86OpCode Push_Ss                   = new x86OpCode("PUSH SS", new byte[] { 0x16 }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Ss                    = new x86OpCode("POP SS", new byte[] { 0x17 }, 0, x86OperandType.None);

        public static readonly x86OpCode Ssb_r32Ptr_r8             = new x86OpCode("SSB", new byte[] { 0x18, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Ssb_r32Ptr_r32            = new x86OpCode("SSB", new byte[] { 0x19, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
        public static readonly x86OpCode Ssb_r8_r32Ptr             = new x86OpCode("SSB", new byte[] { 0x1A, 0x0 }, 0, x86OperandType.Multiple32Or8Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Ssb_r32_r32Ptr            = new x86OpCode("SSB", new byte[] { 0x1B, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Ssb_Al_Byte               = new x86OpCode("SSB AL,", new byte[] { 0x1C }, 1, x86OperandType.Byte);
        public static readonly x86OpCode Ssb_Eax_Dword             = new x86OpCode("SSB EAX,", new byte[] { 0x1D }, 4, x86OperandType.Dword);

        public static readonly x86OpCode Push_Ds                   = new x86OpCode("PUSH DS", new byte[] { 0x1E }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Ds                    = new x86OpCode("POP DS", new byte[] { 0x1F }, 0, x86OperandType.None);

        public static readonly x86OpCode And_r32Ptr_r8             = new x86OpCode("AND", new byte[] { 0x20, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode And_r32Ptr_r32            = new x86OpCode("AND", new byte[] { 0x21, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
        public static readonly x86OpCode And_r8_r32Ptr             = new x86OpCode("AND", new byte[] { 0x22, 0x0 }, 0, x86OperandType.Multiple32Or8Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode And_r32_r32Ptr            = new x86OpCode("AND", new byte[] { 0x23, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode And_Al_Byte               = new x86OpCode("AND AL,", new byte[] { 0x24 }, 1, x86OperandType.Byte);
        public static readonly x86OpCode And_Eax_Dword             = new x86OpCode("AND EAX,", new byte[] { 0x25 }, 4, x86OperandType.Dword);

        /* 0x26 */

        public static readonly x86OpCode Daa                       = new x86OpCode("DAA", new byte[] {0x27}, 0, x86OperandType.None);

        public static readonly x86OpCode Sub_r32Ptr_r8             = new x86OpCode("SUB", new byte[] { 0x28, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Sub_r32Ptr_r32            = new x86OpCode("SUB", new byte[] { 0x29, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
        public static readonly x86OpCode Sub_r8_r32Ptr             = new x86OpCode("SUB", new byte[] { 0x2A, 0x0 }, 0, x86OperandType.Multiple32Or8Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Sub_r32_r32Ptr            = new x86OpCode("SUB", new byte[] { 0x2B, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Sub_Al_Byte               = new x86OpCode("SUB AL,", new byte[] { 0x2C }, 1, x86OperandType.Byte);
        public static readonly x86OpCode Sub_Eax_Dword             = new x86OpCode("SUB EAX,", new byte[] { 0x2D }, 4, x86OperandType.Dword);

        /* 0x2E */

        public static readonly x86OpCode Das                       = new x86OpCode("DAS", new byte[] { 0x2F }, 0, x86OperandType.None);

        public static readonly x86OpCode Xor_r32Ptr_r8             = new x86OpCode("XOR", new byte[] { 0x30, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Xor_r32Ptr_r32            = new x86OpCode("XOR", new byte[] { 0x31, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
        public static readonly x86OpCode Xor_r8_r32Ptr             = new x86OpCode("XOR", new byte[] { 0x32, 0x0 }, 0, x86OperandType.Multiple32Or8Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Xor_r32_r32Ptr            = new x86OpCode("XOR", new byte[] { 0x33, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Xor_Al_Byte               = new x86OpCode("XOR AL,", new byte[] { 0x34 }, 1, x86OperandType.Byte);
        public static readonly x86OpCode Xor_Eax_Dword             = new x86OpCode("XOR EAX,", new byte[] { 0x35 }, 4, x86OperandType.Dword);
        
        /* 0x36 */

        public static readonly x86OpCode Aaa                       = new x86OpCode("AAA", new byte[] { 0x37 }, 0, x86OperandType.None);

        public static readonly x86OpCode Cmp_r32Ptr_r8             = new x86OpCode("CMP", new byte[] { 0x38, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Cmp_r32Ptr_r32            = new x86OpCode("CMP", new byte[] { 0x39, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
        public static readonly x86OpCode Cmp_r8_r32Ptr             = new x86OpCode("CMP", new byte[] { 0x3A, 0x0 }, 0, x86OperandType.Multiple32Or8Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Cmp_r32_r32Ptr            = new x86OpCode("CMP", new byte[] { 0x3B, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Cmp_Al_Byte               = new x86OpCode("CMP AL,", new byte[] { 0x3C }, 1, x86OperandType.Byte);
        public static readonly x86OpCode Cmp_Eax_Dword             = new x86OpCode("CMP EAX,", new byte[] { 0x3D }, 4, x86OperandType.Dword);

        /* 0x3E */

        public static readonly x86OpCode Aas                       = new x86OpCode("AAS", new byte[] { 0x3F }, 0, x86OperandType.None);


        public static readonly x86OpCode Inc_Eax                   = new x86OpCode("INC EAX", new byte[] { 0x40 }, 0, x86OperandType.None);
        public static readonly x86OpCode Inc_Ecx                   = new x86OpCode("INC ECX", new byte[] { 0x41 }, 0, x86OperandType.None);
        public static readonly x86OpCode Inc_Edx                   = new x86OpCode("INC EDX", new byte[] { 0x42 }, 0, x86OperandType.None);
        public static readonly x86OpCode Inc_Ebx                   = new x86OpCode("INC EBX", new byte[] { 0x43 }, 0, x86OperandType.None);
        public static readonly x86OpCode Inc_Esp                   = new x86OpCode("INC ESP", new byte[] { 0x44 }, 0, x86OperandType.None);
        public static readonly x86OpCode Inc_Ebp                   = new x86OpCode("INC EBP", new byte[] { 0x45 }, 0, x86OperandType.None);
        public static readonly x86OpCode Inc_Esi                   = new x86OpCode("INC ESI", new byte[] { 0x46 }, 0, x86OperandType.None);
        public static readonly x86OpCode Inc_Edi                   = new x86OpCode("INC EDI", new byte[] { 0x47 }, 0, x86OperandType.None);

        public static readonly x86OpCode Dec_Eax                   = new x86OpCode("DEC EAX", new byte[] { 0x48 }, 0, x86OperandType.None);
        public static readonly x86OpCode Dec_Ecx                   = new x86OpCode("DEC ECX", new byte[] { 0x49 }, 0, x86OperandType.None);
        public static readonly x86OpCode Dec_Edx                   = new x86OpCode("DEC EDX", new byte[] { 0x4A }, 0, x86OperandType.None);
        public static readonly x86OpCode Dec_Ebx                   = new x86OpCode("DEC EBX", new byte[] { 0x4B }, 0, x86OperandType.None);
        public static readonly x86OpCode Dec_Esp                   = new x86OpCode("DEC ESP", new byte[] { 0x4C }, 0, x86OperandType.None);
        public static readonly x86OpCode Dec_Ebp                   = new x86OpCode("DEC EBP", new byte[] { 0x4D }, 0, x86OperandType.None);
        public static readonly x86OpCode Dec_Esi                   = new x86OpCode("DEC ESI", new byte[] { 0x4E }, 0, x86OperandType.None);
        public static readonly x86OpCode Dec_Edi                   = new x86OpCode("DEC EDI", new byte[] { 0x4F }, 0, x86OperandType.None);

        public static readonly x86OpCode Push_Eax                  = new x86OpCode("PUSH EAX", new byte[] { 0x50 }, 0, x86OperandType.None);
        public static readonly x86OpCode Push_Ecx                  = new x86OpCode("PUSH ECX", new byte[] { 0x51 }, 0, x86OperandType.None);
        public static readonly x86OpCode Push_Edx                  = new x86OpCode("PUSH EDX", new byte[] { 0x52 }, 0, x86OperandType.None);
        public static readonly x86OpCode Push_Ebx                  = new x86OpCode("PUSH EBX", new byte[] { 0x53 }, 0, x86OperandType.None);
        public static readonly x86OpCode Push_Esp                  = new x86OpCode("PUSH ESP", new byte[] { 0x54 }, 0, x86OperandType.None);
        public static readonly x86OpCode Push_Ebp                  = new x86OpCode("PUSH EBP", new byte[] { 0x55 }, 0, x86OperandType.None);
        public static readonly x86OpCode Push_Esi                  = new x86OpCode("PUSH ESI", new byte[] { 0x56 }, 0, x86OperandType.None);
        public static readonly x86OpCode Push_Edi                  = new x86OpCode("PUSH EDI", new byte[] { 0x57 }, 0, x86OperandType.None);

        public static readonly x86OpCode Pop_Eax                   = new x86OpCode("POP EAX", new byte[] { 0x58 }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Ecx                   = new x86OpCode("POP ECX", new byte[] { 0x59 }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Edx                   = new x86OpCode("POP EDX", new byte[] { 0x5A }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Ebx                   = new x86OpCode("POP EBX", new byte[] { 0x5B }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Esp                   = new x86OpCode("POP ESP", new byte[] { 0x5C }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Ebp                   = new x86OpCode("POP EBP", new byte[] { 0x5D }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Esi                   = new x86OpCode("POP ESI", new byte[] { 0x5E }, 0, x86OperandType.None);
        public static readonly x86OpCode Pop_Edi                   = new x86OpCode("POP EDI", new byte[] { 0x5F }, 0, x86OperandType.None);

        public static readonly x86OpCode Pushad                    = new x86OpCode("PUSHAD", new byte[] {0x60},0, x86OperandType.None);
        public static readonly x86OpCode Popad                     = new x86OpCode("POPAD", new byte[] {0x61},0, x86OperandType.None);

        /* 0x62 */
        /* 0x63 */
        /* 0x64 */
        /* 0x65 */
        /* 0x66 */
        /* 0x67 */
        
        public static readonly x86OpCode Push_Dword                = new x86OpCode("PUSH", new byte[] {0x68}, 4, x86OperandType.Dword);

        /* 0x69 */

        public static readonly x86OpCode Push_Byte                 = new x86OpCode("PUSH", new byte[] { 0x6A }, 1, x86OperandType.Byte);
        
        /* 0x6B */
        /* 0x6C */
        /* 0x6D */
        /* 0x6E */
        /* 0x6F */

        public static readonly x86OpCode Jo_Short                  = new x86OpCode("JO SHORT", new byte[] { 0x70 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jno_Short                 = new x86OpCode("JNO SHORT", new byte[] { 0x71 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jb_Short                  = new x86OpCode("JB SHORT", new byte[] { 0x72 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jae_Short                 = new x86OpCode("JAE SHORT", new byte[] { 0x73 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Je_Short                  = new x86OpCode("JE SHORT", new byte[] { 0x74 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jne_Short                 = new x86OpCode("JNE SHORT", new byte[] { 0x75 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jbe_Short                 = new x86OpCode("JBE SHORT", new byte[] { 0x76 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Ja_Short                  = new x86OpCode("JA SHORT", new byte[] { 0x77 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Js_Short                  = new x86OpCode("JS SHORT", new byte[] { 0x78 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jns_Short                 = new x86OpCode("JNS SHORT", new byte[] { 0x79 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jp_Short                  = new x86OpCode("JP SHORT", new byte[] { 0x7A }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jnp_Short                 = new x86OpCode("JNP SHORT", new byte[] { 0x7B }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jl_Short                  = new x86OpCode("JL SHORT", new byte[] { 0x7C }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jnl_Short                 = new x86OpCode("JNL SHORT", new byte[] { 0x7D }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jng_Short                 = new x86OpCode("JNG SHORT", new byte[] { 0x7E }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jg_Short                  = new x86OpCode("JG SHORT", new byte[] { 0x7F }, 1, x86OperandType.ShortInstructionAddress);


        /* 0x80 group */
        public static readonly x86OpCode Add_r32_Byte              = new x86OpCode("ADD", new byte[] { 0x80, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Or_r32_Byte               = new x86OpCode("OR", new byte[] { 0x80, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Adc_r32_Byte              = new x86OpCode("ADC", new byte[] { 0x80, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Ssb_r32_Byte              = new x86OpCode("SSB", new byte[] { 0x80, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode And_r32_Byte              = new x86OpCode("AND", new byte[] { 0x80, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Sub_r32_Byte              = new x86OpCode("SUB", new byte[] { 0x80, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Xor_r32_Byte              = new x86OpCode("XOR", new byte[] { 0x80, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Cmp_r32_Byte              = new x86OpCode("CMP", new byte[] { 0x80, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);

        /* 0x81 group */
        public static readonly x86OpCode Add_r32BytePtr_Dword      = new x86OpCode("ADD", new byte[] { 0x81, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);
        public static readonly x86OpCode Or_r32BytePtr_Dword       = new x86OpCode("OR", new byte[] { 0x81, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);
        public static readonly x86OpCode Adc_r32BytePtr_Dword      = new x86OpCode("ADC", new byte[] { 0x81, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);
        public static readonly x86OpCode Ssb_r32BytePtr_Dword      = new x86OpCode("SSB", new byte[] { 0x81, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);
        public static readonly x86OpCode And_r32BytePtr_Dword      = new x86OpCode("AND", new byte[] { 0x81, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);
        public static readonly x86OpCode Sub_r32BytePtr_Dword      = new x86OpCode("SUB", new byte[] { 0x81, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);
        public static readonly x86OpCode Xor_r32BytePtr_Dword      = new x86OpCode("XOR", new byte[] { 0x81, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);
        public static readonly x86OpCode Cmp_r32BytePtr_Dword      = new x86OpCode("CMP", new byte[] { 0x81, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);

        /* 0x82 group */
        public static readonly x86OpCode Add_r8Orm8_Byte            = new x86OpCode("ADD", new byte[] { 0x82, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Or_r8Orm8_Byte             = new x86OpCode("OR", new byte[] { 0x82, 0x0 }, 4, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Adc_r8Orm8_Byte            = new x86OpCode("ADC", new byte[] { 0x82, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Ssb_r8Orm8_Byte            = new x86OpCode("SSB", new byte[] { 0x82, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode And_r8Orm8_Byte            = new x86OpCode("AND", new byte[] { 0x82, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Sub_r8Orm8_Byte            = new x86OpCode("SUB", new byte[] { 0x82, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Xor_r8Orm8_Byte            = new x86OpCode("XOR", new byte[] { 0x82, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Cmp_r8Orm8_Byte            = new x86OpCode("CMP", new byte[] { 0x82, 0x0 }, 1, x86OperandType.Register32Or8 | x86OperandType.Byte, 1);

        /* 0x83 group */
        public static readonly x86OpCode Add_r32Or8_Byte           = new x86OpCode("ADD", new byte[] { 0x83, 0x0 }, 1, x86OperandType.Register32 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Or_r32Or8_Byte            = new x86OpCode("OR", new byte[] { 0x83, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Adc_r32Or8_Byte           = new x86OpCode("ADC", new byte[] { 0x83, 0x0 }, 1, x86OperandType.Register32 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Ssb_r32Or8_Byte           = new x86OpCode("SSB", new byte[] { 0x83, 0x0 }, 1, x86OperandType.Register32 | x86OperandType.Byte, 1);
        public static readonly x86OpCode And_r32Or8_Byte           = new x86OpCode("AND", new byte[] { 0x83, 0x0 }, 1, x86OperandType.Register32 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Sub_r32Or8_Byte           = new x86OpCode("SUB", new byte[] { 0x83, 0x0 }, 1, x86OperandType.Register32 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Xor_r32Or8_Byte           = new x86OpCode("XOR", new byte[] { 0x83, 0x0 }, 1, x86OperandType.Register32 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Cmp_r32Or8_Byte           = new x86OpCode("CMP", new byte[] { 0x83, 0x0 }, 1, x86OperandType.Register32 | x86OperandType.Byte, 1);


        public static readonly x86OpCode Test_r32_r8               = new x86OpCode("TEST", new byte[] {0x84,0x0}, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Test_r32_r32              = new x86OpCode("TEST", new byte[] {0x85,0x0}, 0, x86OperandType.Multiple32Register, 1);
      
        public static readonly x86OpCode TXchg_r32_r8              = new x86OpCode("XCHG", new byte[] {0x86,0x0}, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode TXchg_r32_r32             = new x86OpCode("XCHG", new byte[] {0x87,0x0}, 0, x86OperandType.Multiple32Register, 1);

        public static readonly x86OpCode Mov_r32Or8                = new x86OpCode("MOV", new byte[] { 0x88, 0x0 }, 0, x86OperandType.Multiple32Or8Register, 1);
        public static readonly x86OpCode Mov_r32Ptr_r32            = new x86OpCode("MOV", new byte[] { 0x89, 0x0 }, 0, x86OperandType.Multiple32Register, 1);
       
        public static readonly x86OpCode Mov_r8_r32Or8Ptr          = new x86OpCode("MOV", new byte[] { 0x8A, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        public static readonly x86OpCode Mov_r32_r32Ptr            = new x86OpCode("MOV", new byte[] { 0x8B, 0x0 }, 0, x86OperandType.Multiple32Register | x86OperandType.OverrideOperandOrder, 1);
        
        /* 0x8C */

        public static readonly x86OpCode Lea                       = new x86OpCode("LEA", new byte[] { 0x8D, 0x0 }, 0, x86OperandType.RegisterLeaRegister, 1);

        /* 0x8E */

        public static readonly x86OpCode Pop_Register              = new x86OpCode("POP", new byte[] { 0x8F, 0x0 }, 0, x86OperandType.Register32, 1);

        public static readonly x86OpCode Nop                       = new x86OpCode("NOP", new byte[] { 0x90 }, 0, x86OperandType.None);
        
        public static readonly x86OpCode Xchg_Eax_Ecx              = new x86OpCode("XCHG EAX, ECX", new byte[] {0x91},0, x86OperandType.None);
        public static readonly x86OpCode Xchg_Eax_Edx              = new x86OpCode("XCHG EAX, ECX", new byte[] {0x92},0, x86OperandType.None);
        public static readonly x86OpCode Xchg_Eax_Ebx              = new x86OpCode("XCHG EAX, ECX", new byte[] {0x93},0, x86OperandType.None);
        public static readonly x86OpCode Xchg_Eax_Esp              = new x86OpCode("XCHG EAX, ECX", new byte[] {0x94},0, x86OperandType.None);
        public static readonly x86OpCode Xchg_Eax_Ebp              = new x86OpCode("XCHG EAX, ECX", new byte[] {0x95},0, x86OperandType.None);
        public static readonly x86OpCode Xchg_Eax_Esi              = new x86OpCode("XCHG EAX, ECX", new byte[] {0x96},0, x86OperandType.None);
        public static readonly x86OpCode Xchg_Eax_Edi              = new x86OpCode("XCHG EAX, ECX", new byte[] {0x97},0, x86OperandType.None);

        public static readonly x86OpCode Cwde                      = new x86OpCode("CWDE", new byte[] {0x98},0, x86OperandType.None);
        public static readonly x86OpCode Cdq                       = new x86OpCode("CDQ", new byte[] {0x99},0, x86OperandType.None);

        public static readonly x86OpCode Call_Far                  = new x86OpCode("CALL FAR", new byte[] {0x9A}, 6, x86OperandType.Fword);

        public static readonly x86OpCode Wait                      = new x86OpCode("WAIT", new byte[] {0x9B}, 0, x86OperandType.None);

        public static readonly x86OpCode Pushfd                    = new x86OpCode("PUSHFD", new byte[] {0x9C}, 0, x86OperandType.None);
        public static readonly x86OpCode Popfd                     = new x86OpCode("POPFD", new byte[] {0x9D}, 0, x86OperandType.None);
        public static readonly x86OpCode Sahf                      = new x86OpCode("SAHF", new byte[] {0x9E}, 0, x86OperandType.None);
        public static readonly x86OpCode Lahf                      = new x86OpCode("LAHF", new byte[] {0x9F}, 0, x86OperandType.None);

        public static readonly x86OpCode Mov_Al_DwordPtr           = new x86OpCode("MOV Al,", new byte[] {0xA0},4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Mov_Eax_DwordPtr          = new x86OpCode("MOV EAX,", new byte[] { 0xA1 }, 4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Mov_DwordPtr_Al           = new x86OpCode("MOV %operand1%, Al", new byte[] { 0xA2 }, 4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Mov_DwordPtr_Eax          = new x86OpCode("MOV %operand1%, EAX", new byte[] { 0xA3 }, 4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        
        /* 0xA4 */
        /* 0xA5 */
        /* 0xA6 */
        /* 0xA7 */

        public static readonly x86OpCode Test_Al_Byte              = new x86OpCode("TEST AL,", new byte[] {0xA8}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Test_Eax_Dword            = new x86OpCode("TEST EAX,", new byte[] {0xA9},4, x86OperandType.Dword);

        public static readonly x86OpCode Stos_BytePtr_Edi          = new x86OpCode("STOS BYTE PTR ES:[EDI]", new byte[] {0xAA}, 0, x86OperandType.None);
        public static readonly x86OpCode Stos_DwordPtr_Edi         = new x86OpCode("STOS DWORD PTR ES:[EDI]", new byte[] {0xAB}, 0, x86OperandType.None);
        public static readonly x86OpCode Lods_BytePtr_Edi          = new x86OpCode("LODS BYTE PTR ES:[EDI]", new byte[] {0xAC}, 0, x86OperandType.None);
        public static readonly x86OpCode Lods_DwordPtr_Edi         = new x86OpCode("LODS DWORD PTR ES:[EDI]", new byte[] {0xAD}, 0, x86OperandType.None);
        public static readonly x86OpCode Scas_BytePtr_Edi          = new x86OpCode("SCAS BYTE PTR ES:[EDI]", new byte[] {0xAE}, 0, x86OperandType.None);
        public static readonly x86OpCode Scas_DwordPtr_Edi         = new x86OpCode("SCAS DWORD PTR ES:[EDI]", new byte[] {0xAF}, 0, x86OperandType.None);

        public static readonly x86OpCode Mov_Al_Byte               = new x86OpCode("MOV AL,",new byte[] {0xB0}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Mov_Cl_Byte               = new x86OpCode("MOV CL,",new byte[] {0xB1}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Mov_Dl_Byte               = new x86OpCode("MOV DL,",new byte[] {0xB2}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Mov_Bl_Byte               = new x86OpCode("MOV BL,",new byte[] {0xB3}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Mov_Ah_Byte               = new x86OpCode("MOV AH,",new byte[] {0xB4}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Mov_Ch_Byte               = new x86OpCode("MOV CH,",new byte[] {0xB5}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Mov_Dh_Byte               = new x86OpCode("MOV DH,",new byte[] {0xB6}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Mov_Bh_Byte               = new x86OpCode("MOV BH,",new byte[] {0xB7}, 1, x86OperandType.Byte);
        
        public static readonly x86OpCode Mov_Eax_Dword             = new x86OpCode("MOV EAX,", new byte[] {0xB8}, 4, x86OperandType.Dword);
        public static readonly x86OpCode Mov_Ecx_Dword             = new x86OpCode("MOV ECX,", new byte[] {0xB9}, 4, x86OperandType.Dword);
        public static readonly x86OpCode Mov_Edx_Dword             = new x86OpCode("MOV EDX,", new byte[] {0xBA}, 4, x86OperandType.Dword);
        public static readonly x86OpCode Mov_Ebx_Dword             = new x86OpCode("MOV EBX,", new byte[] {0xBB}, 4, x86OperandType.Dword);
        public static readonly x86OpCode Mov_Esp_Dword             = new x86OpCode("MOV ESP,", new byte[] {0xBC}, 4, x86OperandType.Dword);
        public static readonly x86OpCode Mov_Ebp_Dword             = new x86OpCode("MOV EBP,", new byte[] {0xBD}, 4, x86OperandType.Dword);
        public static readonly x86OpCode Mov_Esi_Dword             = new x86OpCode("MOV ESI,", new byte[] {0xBE}, 4, x86OperandType.Dword);
        public static readonly x86OpCode Mov_Edi_Dword             = new x86OpCode("MOV EDI,", new byte[] {0xBF}, 4, x86OperandType.Dword);

        public static readonly x86OpCode Rol_BytePtr_Byte          = new x86OpCode("ROL", new byte[] {0xC0,0x0},1, x86OperandType.Register32 | x86OperandType.Byte, 1);
        public static readonly x86OpCode Rol_DwordPtr_Byte         = new x86OpCode("ROL", new byte[] {0xC1,0x0},1, x86OperandType.Register32 | x86OperandType.Byte, 1);

        public static readonly x86OpCode Retn_Word                 = new x86OpCode("RETN", new byte[] {0xC2}, 2, x86OperandType.Word);
        public static readonly x86OpCode Retn                      = new x86OpCode("RETN", new byte[] {0xC3}, 0, x86OperandType.None);

        public static readonly x86OpCode Les_Eax_FwordPtr          = new x86OpCode("LES EAX,", new byte[] {0xC4,0x0},0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Lsd_Eax_FwordPtr          = new x86OpCode("LSD EAX,", new byte[] {0xC5,0x0},0, x86OperandType.Register32, 1);

        public static readonly x86OpCode Mov_BytePtr_Byte          = new x86OpCode("MOV", new byte[] { 0xC6, 0x0 }, 1, x86OperandType.Register32 | x86OperandType.Byte, 1);
        //public static readonly x86OpCode Mov_DwordPtr_Esp_Dword    = new x86OpCode("MOV DWORD PTR [ESP],", new byte[] { 0xC7, 0x4, 0x24 }, 4, x86OperandType.Dword);
        public static readonly x86OpCode Mov_DwordPtr_Dword        = new x86OpCode("MOV", new byte[] { 0xC7, 0x0 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);

        public static readonly x86OpCode Enter                     = new x86OpCode("ENTER", new byte[] {0xC8},6,x86OperandType.WordAndByte);
        public static readonly x86OpCode Leave                     = new x86OpCode("LEAVE", new byte[] {0xC9},0, x86OperandType.None);

        public static readonly x86OpCode Retf_Word                 = new x86OpCode("RETF", new byte[] {0xCA},2,x86OperandType.Word);
        public static readonly x86OpCode Retf                      = new x86OpCode("RETF", new byte[] {0xCB},0,x86OperandType.None);

        public static readonly x86OpCode Int3                      = new x86OpCode("INT3", new byte[] {0xCC},0,x86OperandType.None);
        public static readonly x86OpCode Int_Byte                  = new x86OpCode("INT", new byte[] {0xCD},0, x86OperandType.Byte);
        public static readonly x86OpCode Into                      = new x86OpCode("INTO", new byte[] {0xCE}, 0, x86OperandType.None);
        public static readonly x86OpCode Iretd                     = new x86OpCode("IRETD", new byte[] {0xCF}, 0, x86OperandType.None);

        /* 0xD0 group */
        public static readonly x86OpCode Rol_r8_1                 = new x86OpCode("ROL %operand1%, 1", new byte[] { 0xD0, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Ror_r8_1                 = new x86OpCode("ROR %operand1%, 1", new byte[] { 0xD0, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Rcl_r8_1                 = new x86OpCode("RCL %operand1%, 1", new byte[] { 0xD0, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Rcr_r8_1                 = new x86OpCode("RCR %operand1%, 1", new byte[] { 0xD0, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Shl_r8_1                 = new x86OpCode("SHL %operand1%, 1", new byte[] { 0xD0, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Shr_r8_1                 = new x86OpCode("SHR %operand1%, 1", new byte[] { 0xD0, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Sal_r8_1                 = new x86OpCode("SAL %operand1%, 1", new byte[] { 0xD0, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Sar_r8_1                 = new x86OpCode("SAR %operand1%, 1", new byte[] { 0xD0, 0x0 }, 0, x86OperandType.Register8, 1);

        /* 0xD1 group */
        public static readonly x86OpCode Rol_r32_1                = new x86OpCode("ROL %operand1%, 1", new byte[] { 0xD1, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Ror_r32_1                = new x86OpCode("ROR %operand1%, 1", new byte[] { 0xD1, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Rcl_r32_1                = new x86OpCode("RCL %operand1%, 1", new byte[] { 0xD1, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Rcr_r32_1                = new x86OpCode("RCR %operand1%, 1", new byte[] { 0xD1, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Shl_r32_1                = new x86OpCode("SHL %operand1%, 1", new byte[] { 0xD1, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Shr_r32_1                = new x86OpCode("SHR %operand1%, 1", new byte[] { 0xD1, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Sal_r32_1                = new x86OpCode("SAL %operand1%, 1", new byte[] { 0xD1, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Sar_r32_1                = new x86OpCode("SAR %operand1%, 1", new byte[] { 0xD1, 0x0 }, 0, x86OperandType.Register32, 1);

        /* 0xD2 group */
        public static readonly x86OpCode Rol_r8_Cl                = new x86OpCode("ROL %operand1%, CL", new byte[] { 0xD2, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Ror_r8_Cl                = new x86OpCode("ROR %operand1%, CL", new byte[] { 0xD2, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Rcl_r8_Cl                = new x86OpCode("RCL %operand1%, CL", new byte[] { 0xD2, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Rcr_r8_Cl                = new x86OpCode("RCR %operand1%, CL", new byte[] { 0xD2, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Shl_r8_Cl                = new x86OpCode("SHL %operand1%, CL", new byte[] { 0xD2, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Shr_r8_Cl                = new x86OpCode("SHR %operand1%, CL", new byte[] { 0xD2, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Sal_r8_Cl                = new x86OpCode("SAL %operand1%, CL", new byte[] { 0xD2, 0x0 }, 0, x86OperandType.Register8, 1);
        public static readonly x86OpCode Sar_r8_Cl                = new x86OpCode("SAR %operand1%, CL", new byte[] { 0xD2, 0x0 }, 0, x86OperandType.Register8, 1);

        /* 0xD3 group */
        public static readonly x86OpCode Rol_r32_Cl                = new x86OpCode("ROL %operand1%, CL", new byte[] { 0xD3, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Ror_r32_Cl                = new x86OpCode("ROR %operand1%, CL", new byte[] { 0xD3, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Rcl_r32_Cl                = new x86OpCode("RCL %operand1%, CL", new byte[] { 0xD3, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Rcr_r32_Cl                = new x86OpCode("RCR %operand1%, CL", new byte[] { 0xD3, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Shl_r32_Cl                = new x86OpCode("SHL %operand1%, CL", new byte[] { 0xD3, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Shr_r32_Cl                = new x86OpCode("SHR %operand1%, CL", new byte[] { 0xD3, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Sal_r32_Cl                = new x86OpCode("SAL %operand1%, CL", new byte[] { 0xD3, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Sar_r32_Cl                = new x86OpCode("SAR %operand1%, CL", new byte[] { 0xD3, 0x0 }, 0, x86OperandType.Register32, 1);
        

        public static readonly x86OpCode Aam                       = new x86OpCode("AAM", new byte[] {0xD4}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Aad                       = new x86OpCode("AAD", new byte[] {0xD5}, 1, x86OperandType.Byte);

        public static readonly x86OpCode Salc                      = new x86OpCode("SALC", new byte[] {0xD6},0,x86OperandType.None);

        /* 0xD7 */
        /* 0xD8 */
        /* 0xD9 */
        /* 0xDA */
        /* 0xDB */
        /* 0xDC */
        /* 0xDD */
        /* 0xDE */
        /* 0xDF */

        public static readonly x86OpCode Loopne_Short              = new x86OpCode("LOOPNE SHORT", new byte[] {0xE0}, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Loope_Short               = new x86OpCode("LOOPE SHORT", new byte[] { 0xE1 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Loop_Short                = new x86OpCode("LOOP SHORT", new byte[] { 0xE2 }, 1, x86OperandType.ShortInstructionAddress);
        public static readonly x86OpCode Jecxz_Short               = new x86OpCode("JECXZ SHORT", new byte[] { 0xE3 }, 1, x86OperandType.ShortInstructionAddress);

        public static readonly x86OpCode In_Al_Byte                = new x86OpCode("IN AL," , new byte[] {0xE4}, 1, x86OperandType.Byte);
        public static readonly x86OpCode In_Eax_Byte               = new x86OpCode("IN EAX," , new byte[] {0xE5}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Out_Byte_Al               = new x86OpCode("OUT %operand%,AL" , new byte[] {0xE6}, 1, x86OperandType.Byte);
        public static readonly x86OpCode Out_Byte_Eax              = new x86OpCode("OUT %operand%,EAX" , new byte[] {0xE7}, 1, x86OperandType.Byte);

        public static readonly x86OpCode Call                      = new x86OpCode("CALL", new byte[] {0xE8}, 4, x86OperandType.InstructionAddress);
        public static readonly x86OpCode Jmp                       = new x86OpCode("JMP", new byte[] { 0xE9 }, 4, x86OperandType.InstructionAddress);
        public static readonly x86OpCode Jmp_Far                   = new x86OpCode("JMP FAR", new byte[] { 0xEA }, 6, x86OperandType.InstructionAddress);
        public static readonly x86OpCode Jmp_Short                 = new x86OpCode("JMP SHORT", new byte[] { 0xEB }, 1, x86OperandType.ShortInstructionAddress);

        public static readonly x86OpCode In_Al_Dx                  = new x86OpCode("IN AL,DX", new byte[] {0xEC},0, x86OperandType.None);
        public static readonly x86OpCode In_Eax_Dx                 = new x86OpCode("IN EAX,DX", new byte[] {0xED},0, x86OperandType.None);
        public static readonly x86OpCode Out_Dx_Al                 = new x86OpCode("OUT DX,AL", new byte[] {0xEE},0, x86OperandType.None);
        public static readonly x86OpCode Out_Dx_Eax                = new x86OpCode("OUT DX,EAX", new byte[] {0xEF},0, x86OperandType.None);

        /* 0xF0 */

        public static readonly x86OpCode Int1                      = new x86OpCode("INT1", new byte[]{ 0xF1}, 0, x86OperandType.None);
        
        /* 0xF2 */

        public static readonly x86OpCode Rep                       = new x86OpCode("REP", new byte[] { 0xF3 }, 0, x86OperandType.Instruction);
        public static readonly x86OpCode Hlt                       = new x86OpCode("HLT", new byte[] {0xF4},0, x86OperandType.None);
        public static readonly x86OpCode Cmc                       = new x86OpCode("CMC", new byte[] {0xF5},0, x86OperandType.None);

        /* 0xF6 */

        /* 0xF7 group */
        public static readonly x86OpCode Test_r32_Dword            = new x86OpCode("TEST", new byte[] { 0xF7, 0x00 }, 4, x86OperandType.Register32 | x86OperandType.Dword, 1);
        internal static readonly x86OpCode __INVALID_F7            = new x86OpCode("DB F7", new byte[] { 0xF7 }, 0, x86OperandType.None) { _isValid = false };
        public static readonly x86OpCode Not_r32                   = new x86OpCode("NOT", new byte[] { 0xF7, 0x10 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Neg_r32                   = new x86OpCode("NEG", new byte[] { 0xF7, 0x18 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Mul_r32                   = new x86OpCode("MUL", new byte[] { 0xF7, 0x20 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode IMul_r32                  = new x86OpCode("IMUL", new byte[] { 0xF7, 0x28 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Div_r32                   = new x86OpCode("DIV", new byte[] { 0xF7, 0x30 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode IDiv_r32                  = new x86OpCode("IDIV", new byte[] { 0xF7, 0x38 }, 0, x86OperandType.Register32, 1);


        public static readonly x86OpCode Clc                       = new x86OpCode("HLT", new byte[] {0xF8},0, x86OperandType.None);
        public static readonly x86OpCode Stc                       = new x86OpCode("CMC", new byte[] {0xF9},0, x86OperandType.None);
        public static readonly x86OpCode Cli                       = new x86OpCode("CLI", new byte[] {0xFA},0, x86OperandType.None);
        public static readonly x86OpCode Sti                       = new x86OpCode("STI", new byte[] {0xFB},0, x86OperandType.None);
        public static readonly x86OpCode Cld                       = new x86OpCode("CLD", new byte[] {0xFC},0, x86OperandType.None);
        public static readonly x86OpCode Std                       = new x86OpCode("STD", new byte[] {0xFD},0, x86OperandType.None);

       
        /* 0xFE group */
        public static readonly x86OpCode Inc_BytePtr               = new x86OpCode("INC", new byte[] { 0xFE, 0x0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Dec_BytePtr               = new x86OpCode("DEC", new byte[] { 0xFE, 0x8 }, 0, x86OperandType.Register32, 1);


        /* 0xFF group */
        public static readonly x86OpCode Inc_DwordPtr_Register     = new x86OpCode("INC", new byte[] { 0xFF, 0x0 }, 0, x86OperandType.Register32 | x86OperandType.ForceDwordPointer, 1);
        public static readonly x86OpCode Inc_DwordPtr              = new x86OpCode("INC", new byte[] {0xFF, 0x5}, 4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Dec_DwordPtr_Register     = new x86OpCode("DEC", new byte[] { 0xFF, 0x8 }, 0, x86OperandType.Register32 | x86OperandType.ForceDwordPointer, 1);
        public static readonly x86OpCode Dec_DwordPtr              = new x86OpCode("DEC", new byte[] { 0xFF, 0xD }, 4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Call_DwordPtr_Register    = new x86OpCode("CALL", new byte[] { 0xFF, 0x10 }, 0, x86OperandType.Register32 | x86OperandType.ForceDwordPointer, 1);
        public static readonly x86OpCode Call_DwordPtr             = new x86OpCode("CALL", new byte[] { 0xFF, 0x15 }, 4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Jmp_DwordPtr_Register     = new x86OpCode("JMP", new byte[] { 0xFF, 0x20 }, 0, x86OperandType.Register32 | x86OperandType.ForceDwordPointer, 1);
        public static readonly x86OpCode Jmp_DwordPtr              = new x86OpCode("JMP", new byte[] { 0xFF, 0x25 }, 4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Jmp_Far_FwordPtr_Register = new x86OpCode("JMP FAR", new byte[] { 0xFF, 0x28 }, 0, x86OperandType.Register32 | x86OperandType.ForceDwordPointer, 1);
        public static readonly x86OpCode Jmp_Far_FwordPtr          = new x86OpCode("JMP FAR", new byte[] { 0xFF, 0x2D }, 6, x86OperandType.Fword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Push_DwordPtr_Register    = new x86OpCode("PUSH", new byte[] { 0xFF, 0x30 }, 0, x86OperandType.Register32 | x86OperandType.ForceDwordPointer, 1);
        public static readonly x86OpCode Push_DwordPtr             = new x86OpCode("PUSH", new byte[] { 0xFF, 0x35 }, 4, x86OperandType.Dword | x86OperandType.ForceDwordPointer);
        public static readonly x86OpCode Call_Register             = new x86OpCode("CALL", new byte[] { 0xFF, 0xD0 }, 0, x86OperandType.Register32, 1);
        public static readonly x86OpCode Jmp_Register              = new x86OpCode("JMP", new byte[] { 0xFF, 0xE0 }, 0, x86OperandType.Register32, 1);
    }
}
