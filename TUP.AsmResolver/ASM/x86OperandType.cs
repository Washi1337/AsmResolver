using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// An enumeration that represents all supported operand types of x86 assembly instructions.
    /// </summary>
    [Flags]
    public enum x86OperandType : uint
    {
        //
        // Normal operand values (0 - 7 bits)
        //

        NormalOperandMask = 0x000000FF,
        
        /// <summary>
        /// The instruction doesn't have any operands.
        /// </summary>
        None = 0x00000001,
        /// <summary>
        /// The instruction has got a single byte operand.
        /// </summary>
        Byte = 0x00000002,
        /// <summary>
        /// The instruction has got a single word operand.
        /// </summary>
        Word = 0x00000003,
        /// <summary>
        /// The instruction has got a single dword operand.
        /// </summary>
        Dword = 0x00000004,
        /// <summary>
        /// The instruction has got a single fword operand.
        /// </summary>
        Fword = 0x00000005,
        /// <summary>
        /// The instruction has got a single qword operand.
        /// </summary>
        Qword = 0x00000006,
        /// <summary>
        /// The instruction has got a word and byte operand.
        /// </summary>
        WordAndByte = 0x00000007,
        /// <summary>
        /// The instruction has got an instruction address operand.
        /// </summary>
        InstructionAddress = 0x00000008,
        /// <summary>
        /// The instruction has got a short instruction address operand.
        /// </summary>
        ShortInstructionAddress = 0x00000009,
        /// <summary>
        /// The instruction is a prefix to another instruction.
        /// </summary>
        Instruction = 0x0000000A,

        // 
        // Register operand values (8 - 15 bits)
        //

        RegisterOperandMask = 0x0000FF00,

        /// <summary>
        /// The instruction has got a single 32 bit register operand.
        /// </summary>
        Register32 = 0x00001000,
        /// <summary>
        /// The instruction has got a single 8 bit register operand.
        /// </summary>
        Register8 = 0x00002000,
        /// <summary>
        /// The instruction has got a single 32 or 8 bit register operand.
        /// </summary>
        Register32Or8 = 0x00003000,
        /// <summary>
        /// The instruction has got multiple 32 bit registers
        /// </summary>
        Multiple32Register = 0x00004000,
        /// <summary>
        /// The instruction has got multiple 16 bit registers
        /// </summary>
        Multiple16Register = 0x00005000,
        /// <summary>
        /// The instruction has got a pair of 32 bit, 8 bit, or 32 and 8 bit registers.
        /// </summary>
        Multiple32Or8Register  = 0x00006000,
        /// <summary>
        /// The instruction has got a register and a LEA register.
        /// </summary>
        RegisterLeaRegister = 0x00007000,

        //
        // Override values (16 - 23 bits)
        //

        OverridingOperandMask = 0x00FF0000,

        /// <summary>
        /// Indicates the instruction has got two operands which are in an inverted order.
        /// </summary>
        OverrideOperandOrder = 0x00010000,

        ForceDwordPointer = 0x00020000,
        
        
    }
}
