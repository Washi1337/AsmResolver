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

        public static MSILInstruction Create(MSILOpCode opcode, uint metadataToken, NETHeader netHeader)
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
                    case OperandType.Variable:
                        size += 2;
                        break;
                    case OperandType.Field:
                    case OperandType.Int32:
                    case OperandType.Float32:
                    case OperandType.InstructionTarget:
                    case OperandType.Method:
                    case OperandType.Signature:
                    case OperandType.String:
                    case OperandType.Token:
                    case OperandType.Type:
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

        public int CalculateStackModification()
        {
            int stackModification = 0;
            switch (OpCode.StackBehaviourPop)
            {
                case StackBehaviour.Pop1:
                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                    stackModification--;
                    break;
                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    stackModification -= 2;
                    break;
                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_pop1:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                    stackModification -= 3;
                    break;
                case StackBehaviour.Varpop:
                    if (Operand is MethodReference)
                    {
                        MethodReference methodRef = Operand as MethodReference;
                        if (methodRef.Signature != null)
                        {
                            if (methodRef.Signature.HasParameters)
                                stackModification -= methodRef.Signature.Parameters.Length;

                            if (methodRef.Signature.HasThis)
                                stackModification--;
                        }
                    }
                    break;
            }

            switch (OpCode.StackBehaviourPush)
            {
                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                    stackModification++;
                    break;
                case StackBehaviour.Push1_push1:
                    stackModification += 2;
                    break;
                case StackBehaviour.Varpush:
                    if (Operand is MethodReference)
                    {
                        MethodReference methodRef = Operand as MethodReference;
                        if (methodRef.Signature != null)
                            if (methodRef.Signature.ReturnType.FullName != "System.Void")
                                stackModification++;
                    }
                    break;
            }

            return stackModification;
        }

        public override string ToString()
        {
            return "IL_" + Offset.ToString("X4") +": " +OpCode.Name + (Operand != null ? " " +GetOperandString() : "");
        }

    }
}
