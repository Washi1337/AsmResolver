using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MSILInstruction
    {
        internal MSILInstruction()
        {
        }

        internal MSILInstruction(int offset, MSILOpCode opcode, byte[] rawOperand, object operand)
        {
            Offset = offset;
            OpCode = opcode;
            Operand = operand;
            OperandBytes = rawOperand;
        }

        public static MSILInstruction Create(MSILOpCode opcode, object operand)
        {
            MSILInstruction instruction = new MSILInstruction()
            {
                OpCode = opcode,
                Operand = operand,
            };
            return instruction;
        }

        public static MSILInstruction Create(MSILOpCode opcode, int metadataToken, NETHeader netHeader)
        {
            return new MSILInstruction() { OpCode = opcode, OperandBytes = BitConverter.GetBytes(metadataToken), Operand = netHeader.TokenResolver.ResolveToken(metadataToken) };
        }

        public MSILInstruction Next { get; internal set; }
        public MSILInstruction Previous { get; internal set; }

        public int Offset { get; internal set; }
        public MSILOpCode OpCode { get; internal set; }
        public object Operand { get; internal set; }
        public byte[] OperandBytes { get; internal set; }
        public int Size
        {
            get
            {
                int size = OpCode.Bytes.Length;
                switch (OpCode.OperandType)
                {
                    case OperandType.Int8:
                    case OperandType.ShortArgument:
                    case OperandType.ShortInstructionTarget:
                    case OperandType.ShortVariable:
                        size += 1;
                        break;
                    case OperandType.Argument:
                    case OperandType.Field:
                    case OperandType.Int32:
                    case OperandType.Float32:
                    case OperandType.InstructionTarget:
                    case OperandType.Method:
                    case OperandType.Signature:
                    case OperandType.String:
                    case OperandType.Token:
                    case OperandType.Type:
                    case OperandType.Variable:
                        size += 4;
                        break;
                    case OperandType.Float64:
                    case OperandType.Int64:
                        size += 8;
                        break;
                    case OperandType.InstructionTable:
                        size += 4;
                        size += ((int[])Operand).Length * 4;
                        break;
                }
                return size;
            }
        }

        public string GetOperandString()
        {
            if (Operand == null)
                return string.Empty;

            switch (OpCode.OperandType)
            {
                case OperandType.InstructionTarget:
                case OperandType.ShortInstructionTarget:
                    if (Operand is int)
                        return "IL_" + ((int)Operand).ToString("X4");
                    else
                        return "IL_" + (Operand as MSILInstruction).Offset.ToString("X4");
                case OperandType.InstructionTable:
                    string targets = "{";
                    int[] offsets = (int[])Operand;

                    for (int i =0; i < offsets.Length; i++)
                        targets+= "IL_" + offsets[i].ToString("X4") + (i == offsets.Length-1? "": ", ");

                    targets += "}";
                    return targets;
                case OperandType.String:
                    return "\"" + Operand.ToString() + "\"";
                case OperandType.None:
                    return "";
                default:
                    return Operand == null ? "" : Operand.ToString();

            }
        }

        public override string ToString()
        {
            return "IL_" + Offset.ToString("X4") +": " +OpCode.Name + (Operand != null ? " " +GetOperandString() : "");
        }

    }
}
