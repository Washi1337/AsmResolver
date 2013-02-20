using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// An enumeration that represents all supported operand types of x86 assembly instructions.
    /// </summary>
    public enum x86OperandType
    {
        /// <summary>
        /// The instruction doesn't have any operands.
        /// </summary>
        None,
        /// <summary>
        /// The instruction has got a single byte operand.
        /// </summary>
        Byte,
        /// <summary>
        /// The instruction has got a single word operand.
        /// </summary>
        Word,
        /// <summary>
        /// The instruction has got a single dword operand.
        /// </summary>
        Dword,
        /// <summary>
        /// The instruction has got a single dword pointer operand.
        /// </summary>
        DwordPtr,
        /// <summary>
        /// The instruction has got a single fword operand.
        /// </summary>
        Fword,
        /// <summary>
        /// The instruction has got a single fword pointer operand.
        /// </summary>
        FwordPtr,
        /// <summary>
        /// The instruction has got a single qword operand.
        /// </summary>
        Qword,
        /// <summary>
        /// The instruction has got a word and byte operand.
        /// </summary>
        WordAndByte,
        /// <summary>
        /// The instruction has got a single register operand.
        /// </summary>
        Register,
        /// <summary>
        /// The instruction has got a single register pointer operand.
        /// </summary>
        RegisterPointer,
        /// <summary>
        /// The instruction has got a register and byte operand.
        /// </summary>
        RegisterAndByte,
        /// <summary>
        /// The instruction has got a register and dword operand.
        /// </summary>
        RegisterAndDword,
        /// <summary>
        /// The instruction has got a register and offset operand.
        /// </summary>
        RegisterOffset,
        /// <summary>
        /// The instruction has got multiple 32 bit registers
        /// </summary>
        Multiple32Register,
        /// <summary>
        /// The instruction has got multiple 16 bit registers
        /// </summary>
        Multiple16Register,
        /// <summary>
        /// The instruction has got a pair of 32 bit, 8 bit, or 32 and 8 bit registers.
        /// </summary>
        Multiple32Or8Register,
        /// <summary>
        /// The instruction has got a register and a LEA register.
        /// </summary>
        RegisterLeaRegister,
        /// <summary>
        /// The instruction has got an instruction address operand.
        /// </summary>
        InstructionAddress,
        /// <summary>
        /// The instruction has got a short instruction address operand.
        /// </summary>
        ShortInstructionAddress,
        /// <summary>
        /// The instruction is a prefix to another instruction.
        /// </summary>
        Instruction
    }
}
