using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MSILDisassembler  
    {
        PE.PeImage image;
        uint ilOffset;
        public MethodBody MethodBody { get; internal set; }
        MetaDataTokenResolver tokenresolver;
        internal static MSILOpCode[] opcodes;

        static MSILDisassembler()
        {
            LoadOpCodes();
        }
        public MSILDisassembler(MethodBody body)
        {
            MethodBody = body;
            
            Section section = Section.GetSectionByRva(body.Method.netheader.assembly, body.Method.RVA);

            ilOffset = new OffsetConverter(section).RvaToFileOffset(body.Method.RVA) + (uint)body.HeaderSize;

            image = section.ParentAssembly.peImage;
            tokenresolver = new MetaDataTokenResolver(body.Method.netheader);
        }

        private static void LoadOpCodes()
        {
            FieldInfo[] fields = typeof(MSILOpCodes).GetFields();
            opcodes = new MSILOpCode[fields.Length];

            for (int i = 0; i < fields.Length; i++)
                opcodes[i] = (MSILOpCode)fields[i].GetValue(null);

        }
        public MSILInstruction[] Disassemble()
        {
            return Disassemble(0, MethodBody.CodeSize);
        }
        public MSILInstruction[] Disassemble(int startoffset, int length)
        {
            List<MSILInstruction> instructions = new List<MSILInstruction>();
            image.SetOffset(ilOffset + startoffset);
            int currentOffset = 0;
            while (image.Position < ilOffset + startoffset + length)
            {
                MSILInstruction instruction = DisassembleNextInstruction();
                instructions.Add(instruction);
                currentOffset += instruction.Size;
                image.SetOffset(ilOffset + startoffset + currentOffset);

            }
            SetEnclosingInstructions(instructions);
            SetBranchTargets(instructions);
            return instructions.ToArray();
        }
        public MSILInstruction DisassembleNextInstruction()
        {
            int currentOffset = (int)(image.Position - ilOffset);

            MSILOpCode opcode = ReadNextOpCode();

            byte[] rawoperand = ReadRawOperand(opcode);
            object operand = ConvertToOperand(currentOffset, opcode, rawoperand);

            return new MSILInstruction(currentOffset, opcode, rawoperand, operand);

        }
        public int CurrentOffset
        {
            get{
                return (int)(image.Position - ilOffset);
            }
            set
            {
                image.SetOffset(ilOffset + value);
            }
        }

        private MSILOpCode ReadNextOpCode()
        {           

            int indexToCheck = 0;
            byte opcodebyte = image.ReadByte();

            if (opcodebyte == 0xFE)
            {
                indexToCheck = 1;
                opcodebyte = image.ReadByte();
            }

            foreach (MSILOpCode opcode in opcodes)
            {
                if (opcode.Bytes.Length == indexToCheck + 1 && opcode.Bytes[indexToCheck] == opcodebyte)
                {
                    return opcode;
                }
            }
            return new MSILOpCode("<unknown>", (indexToCheck == 1 ? new byte[] {0xFE, opcodebyte} : new byte[] { opcodebyte }), OperandType.None);
        }
        private byte[] ReadRawOperand(MSILOpCode opcode)
        {
            int size = opcode.Bytes.Length;
            switch (opcode.OperandType)
            {
                case OperandType.Int8:
                case OperandType.ShortArgument:
                case OperandType.ShortInstructionTarget:
                case OperandType.ShortVariable:
                    return image.ReadBytes(1);
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
                    return image.ReadBytes(4);
                case OperandType.Float64:
                case OperandType.Int64:
                    return image.ReadBytes(8);
                case OperandType.InstructionTable:
                    byte[] header = image.ReadBytes(4);
                    byte[] offsets = image.ReadBytes(BitConverter.ToInt32(header, 0) * sizeof(int));
                    return ASMGlobals.MergeBytes(header, offsets);
            }
            return null;
        }
        private object ConvertToOperand(int instructionOffset, MSILOpCode opcode, byte[] rawoperand)
        {
            try
            {
                switch (opcode.OperandType)
                {
                    case OperandType.Argument:
                        return GetParameter(BitConverter.ToInt32(rawoperand, 0));
                    case OperandType.ShortArgument:
                        return GetParameter(rawoperand[0]);

                    case OperandType.Float32:
                        return BitConverter.ToSingle(rawoperand, 0);
                    case OperandType.Float64:
                        return BitConverter.ToDouble(rawoperand, 0);
                    case OperandType.InstructionTable:

                        int length = BitConverter.ToInt32(rawoperand, 0);
                        int[] offsets = new int[length];
                        int nextOffset = instructionOffset + (length * 4) + opcode.Bytes.Length + 4;
                        for (int i = 0; i < length; i++)
                        {
                            int index = (i + 1) * sizeof(int);
                            int roffset = BitConverter.ToInt32(rawoperand, index);
                            offsets[i] = roffset + nextOffset;
                        }

                        return offsets;

                    case OperandType.InstructionTarget:
                        return BitConverter.ToInt32(rawoperand, 0) + instructionOffset + opcode.Bytes.Length + sizeof(int);


                    case OperandType.ShortInstructionTarget:
                        return ASMGlobals.ByteToSByte(rawoperand[0]) + instructionOffset + opcode.Bytes.Length + sizeof(byte);

                    case OperandType.Int8:
                        return ASMGlobals.ByteToSByte(rawoperand[0]);
                    case OperandType.Int32:
                        return BitConverter.ToInt32(rawoperand, 0);
                    case OperandType.Int64:
                        return BitConverter.ToInt64(rawoperand, 0);
                    case OperandType.Token:
                    case OperandType.Field:
                    case OperandType.Method:
                    case OperandType.Type:
                        int metadata = BitConverter.ToInt32(rawoperand, 0);
                        try
                        {
                            object operand = tokenresolver.ResolveMember(metadata);

                            if ((operand is TypeSpecification) && (operand as TypeSpecification).OriginalType is GenericParamReference)
                            {
                                GenericParamReference paramRef = (operand as TypeSpecification).OriginalType as GenericParamReference;
                                if (paramRef.IsMethodVar)
                                {
                                    if (MethodBody.Method.GenericParameters != null && MethodBody.Method.GenericParameters.Length > 0)
                                        operand = MethodBody.Method.GenericParameters[paramRef.Index];
                                }
                                else
                                {
                                    if (MethodBody.Method.DeclaringType.GenericParameters != null && MethodBody.Method.DeclaringType.GenericParameters.Length > 0)
                                        operand = MethodBody.Method.DeclaringType.GenericParameters[paramRef.Index];
                                }
                            }
                            return operand;
                        }
                        catch { return new TypeReference() { name = "TOKEN:" + metadata.ToString("X8") }; }

                    case OperandType.ShortVariable:
                        return GetVariable(ASMGlobals.ByteToSByte(rawoperand[0]));
                    case OperandType.Variable:
                        return GetVariable(BitConverter.ToInt32(rawoperand, 0));
                    case OperandType.Signature:
                        return BitConverter.ToInt32(rawoperand, 0);
                    case OperandType.String:
                        return tokenresolver.ResolveString(BitConverter.ToInt32(rawoperand, 0));
                }
            }
            catch { }

            return null;


        }

        private void SetBranchTargets(List<MSILInstruction> instructions)
        {
            for (int i = 0; i < instructions.Count; i++)
            {
                MSILInstruction instruction = instructions[i];
                if (instruction.OpCode.OperandType == OperandType.InstructionTarget || instruction.OpCode.OperandType == OperandType.ShortInstructionTarget)
                {
                    int targetOffset = (int)instruction.Operand;
                    MSILInstruction targetInstruction = instructions.FirstOrDefault(t => t.Offset == targetOffset);
                    if (targetInstruction != null)
                        instructions[i].Operand = targetInstruction;
                }
            }
        }
        private void SetEnclosingInstructions(List<MSILInstruction> instructions)
        {
            for (int i = 0; i < instructions.Count; i++)
            {
                if (i > 0)
                    instructions[i].Previous = instructions[i - 1];
                if (i < instructions.Count - 1)
                    instructions[i].Next = instructions[i + 1];
            }
        }

        private VariableDefinition GetVariable(int index)
        {
            if (index >= 0 && index <= MethodBody.Variables.Length)
                return MethodBody.Variables[index];
            return null;
        }
        private ParameterDefinition GetParameter(int index)
        {
            if (!MethodBody.Method.Attributes.HasFlag(MethodAttributes.Static))
                index--;
            if (index >= 0 && index <= MethodBody.Method.Parameters.Length)
                return MethodBody.Method.Parameters[index];
            return null;
        }

    }
}
