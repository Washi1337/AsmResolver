using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.Exceptions;

namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// Represents a 32-bit assembly instruction.
    /// </summary>
    public class x86Instruction
    {
        internal x86Instruction(Win32Assembly assembly)
        {
            this.assembly = assembly;
            OpCode = x86OpCodes.Unknown;
            Offset = new Offset(0, 0,0,  OperandType.Normal);
        }
        /// <summary>
        /// Creates an instance of a x86 instruction without an operand.
        /// </summary>
        /// <param name="assembly">The parent assembly.</param>
        /// <param name="opcode">The opcode to use.</param>
        /// <returns></returns>
        public static x86Instruction Create(Win32Assembly assembly, x86OpCode opcode)
        {
            return Create(assembly, opcode, null, null);
        }
        /// <summary>
        /// Creates an instance of a x86 instruction with a single operand.
        /// </summary>
        /// <param name="assembly">The parent assembly.</param>
        /// <param name="opcode">The opcode to use.</param>
        /// <param name="operand">The operand to use.</param>
        /// <returns></returns>
        public static x86Instruction Create(Win32Assembly assembly, x86OpCode opcode, Operand operand)
        {
            return Create(assembly, opcode, operand, null);
        }
        /// <summary>
        /// Creates an instance of a x86 instruction with two operands.
        /// </summary>
        /// <param name="assembly">The parent assembly.</param>
        /// <param name="opcode">The opcode to use.</param>
        /// <param name="operand1">The first operand to use.</param>
        /// <param name="operand2">The second operand to use.</param>
        /// <returns></returns>
        public static x86Instruction Create(Win32Assembly assembly, x86OpCode opcode, Operand operand1, Operand operand2)
        {
            x86Instruction newInstruction = new x86Instruction(assembly);
            newInstruction.OpCode = opcode;
            newInstruction.operand1 = operand1;
            newInstruction.operand2 = operand2;
            newInstruction.GenerateBytes();
            return newInstruction;
        }






        #region Variables

        internal Win32Assembly assembly;
        internal x86OpCode code;
        internal Operand operand1;
        internal Operand operand2;
        internal byte[] operandbytes;

        #endregion

        #region Properties
        /// <summary>
        /// Gets the opcode of the assembly instruction.
        /// </summary>
        public x86OpCode OpCode
        {
            get
            {
                return code;
            }
            internal set
            {
                code = value;
            }
        }
        /// <summary>
        /// Gets the first operand (if available) of the assembly instruction.
        /// </summary>
        public Operand Operand1
        {
            get
            {
                return operand1;
            }
            internal set
            {
                operand1 = value;
            }
        }
        /// <summary>
        /// Gets the second operand (if available) of the assembly instruction.
        /// </summary>
        public Operand Operand2
        {
            get
            {
                return operand2;
            }
            internal set
            {
                operand2 = value;
            }
        }
        /// <summary>
        /// Gets the operand bytes of the assembly instruction.
        /// </summary>
        public byte[] OperandBytes
        {
            get
            {
                return operandbytes;
            }
        }

        /// <summary>
        /// Gets the offset of the instruction.
        /// </summary>
        public Offset Offset
        {
            get;
            internal set;

        }
        /// <summary>
        /// Gets the size in bytes of the instruction.
        /// </summary>
        public int Size
        {
            get
            {
                return OpCode.opcodebytes.Length + OpCode.operandlength;
            }
        }


        #endregion

        #region Methods

        /// <summary>
        /// Returns a raw string representation of the instruction.
        /// </summary>
        /// <returns></returns>
        public string ToAsmString()
        {
            return ToAsmString(false);
        }
        /// <summary>
        /// Returns a string representation of the instruction.
        /// </summary>
        /// <param name="virtualString">A boolean value that indicates all offsets and operands should be in virtual format.</param>
        /// <returns></returns>
        public string ToAsmString(bool virtualString)
        {
            string operand1 = this.operand1 == null ? "" : this.operand1.ToString(virtualString);
            string operand2 = this.operand2 == null ? "" : this.operand2.ToString(virtualString);
            
            
            return code.ToString() + " " +  operand1 + (operand2 == "" ? "" : ", " + operand2);
        }

        


        /// <summary>
        /// Returns a readable assembly instruction, containing the offset, opcode and operand.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //if (code.Name.ToLower().StartsWith("add %operand%"))
                //System.Diagnostics.Debugger.Break();
            return Offset.ToString(false) + ": " + ToAsmString(false);
        }

        private void GenerateBytes()
        {
            switch (code.operandtype)
            {
                case x86OperandType.None:
                    break;
                case x86OperandType.Byte:
                    operandbytes = new byte[] { (byte)operand1.Value };
                    break;
                case x86OperandType.Word:
                    operandbytes = BitConverter.GetBytes((short)operand1.Value);
                    break;
                case x86OperandType.Dword:
                case x86OperandType.DwordPtr:
                    operandbytes = BitConverter.GetBytes((int)operand1.Value);
                    break;
                case x86OperandType.Qword:
                    operandbytes = BitConverter.GetBytes((long)operand1.Value);
                    break;
                case x86OperandType.InstructionAddress:
                    Offset targetOffset = ((Offset)operand1.Value);
                    int difference = (int)((targetOffset.FileOffset + this.Size) - Offset.FileOffset);
                    operandbytes = BitConverter.GetBytes(difference);
                    break;
                case x86OperandType.ShortInstructionAddress:
                    targetOffset = ((Offset)operand1.Value);
                    difference = (int)((targetOffset.FileOffset + this.Size) - Offset.FileOffset);
                    operandbytes = new byte[] { (byte)difference };
                    break;
                case x86OperandType.Register:
                    code.opcodebytes[code.variableByteIndex] += (byte)(x86Register)operand1.Value;
                    break;
                default:
                    throw new NotImplementedException("The instruction bytes could not be created because the operand type is not supported yet.");
            }
        }

        #endregion
    }
}
