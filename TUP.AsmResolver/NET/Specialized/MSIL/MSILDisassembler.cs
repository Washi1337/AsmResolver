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
        private BinaryReader _reader;
        private uint _ilOffset;
        internal static MSILOpCode[] _opCodes;

        static MSILDisassembler()
        {
            LoadOpCodes();
        }       
        
        private static void LoadOpCodes()
        {
            FieldInfo[] fields = typeof(MSILOpCodes).GetFields();
            _opCodes = new MSILOpCode[fields.Length];

            for (int i = 0; i < fields.Length; i++)
                _opCodes[i] = (MSILOpCode)fields[i].GetValue(null);

        }

        public MSILDisassembler(MethodBody body)
        {
            MethodBody = body;
            
            Section section = Section.GetSectionByRva(body.Method._netheader._assembly, body.Method.RVA);

            _ilOffset = new OffsetConverter(section).RvaToFileOffset(body.Method.RVA) + (uint)body.HeaderSize;

            _reader = section.ParentAssembly._peImage.Reader;
            TokenResolver = new MetaDataTokenResolver(body.Method._netheader);
        }

        public MSILDisassembler(byte[] bytes, MetaDataTokenResolver tokenResolver)
            :this(new MemoryStream(bytes), tokenResolver)
        {
        }

        public MSILDisassembler(Stream stream, MetaDataTokenResolver tokenResolver)
        {
            TokenResolver = tokenResolver;
            _reader = new BinaryReader(stream);
        }

        /// <summary>
        /// Gets or sets the current relative IL offset.
        /// </summary>
        public int CurrentOffset
        {
            get
            {
                return (int)(_reader.BaseStream.Position - _ilOffset);
            }
            set
            {
                _reader.BaseStream.Position = _ilOffset + value;
            }
        }
       
        public MethodBody MethodBody 
        {
            get; 
            internal set; 
        }

        public MetaDataTokenResolver TokenResolver
        {
            get;
            private set;
        }

        public bool IsDynamic
        {
            get { return MethodBody != null; }
        }

        public MSILInstruction[] Disassemble()
        {
            return Disassemble(0, (int)MethodBody.CodeSize);
        }

        public MSILInstruction[] Disassemble(int startoffset, int length)
        {
            List<MSILInstruction> instructions = new List<MSILInstruction>();
            _reader.BaseStream.Position = _ilOffset + startoffset;
            int currentOffset = 0;
            while (_reader.BaseStream.Position < _ilOffset + startoffset + length)
            {
                MSILInstruction instruction = DisassembleNextInstruction();
                instructions.Add(instruction);
                currentOffset += instruction.Size;
                _reader.BaseStream.Position = _ilOffset + startoffset + currentOffset;

            }
            SetEnclosingInstructions(instructions);
            SetBranchTargets(instructions);
            return instructions.ToArray();
        }

        public MSILInstruction DisassembleNextInstruction()
        {
            int currentOffset = (int)(_reader.BaseStream.Position - _ilOffset);

            //if (currentOffset == 0x64)
            //    System.Diagnostics.Debugger.Break();

            MSILOpCode opcode = ReadNextOpCode();

            byte[] rawoperand = ReadRawOperand(opcode);
            object operand = ConvertToOperand(currentOffset, opcode, rawoperand);

            return new MSILInstruction(currentOffset, opcode, rawoperand, operand);

        }

        private MSILOpCode ReadNextOpCode()
        {           

            int indexToCheck = 0;
            byte opcodebyte = _reader.ReadByte();

            if (opcodebyte == 0xFE)
            {
                indexToCheck = 1;
                opcodebyte = _reader.ReadByte();
            }

            foreach (MSILOpCode opcode in _opCodes)
            {
                if (opcode.Bytes.Length == indexToCheck + 1 && opcode.Bytes[indexToCheck] == opcodebyte)
                {
                    return opcode;
                }
            }
            return new MSILOpCode("<unknown>", (indexToCheck == 1 ? new byte[] {0xFE, opcodebyte} : new byte[] { opcodebyte }), OperandType.None, StackBehaviour.Push0 | StackBehaviour.Pop0);
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
                    return _reader.ReadBytes(1);

                case OperandType.Argument:
                case OperandType.Variable:
                    return _reader.ReadBytes(2);

                case OperandType.Field:
                case OperandType.Int32:
                case OperandType.Float32:
                case OperandType.InstructionTarget:
                case OperandType.Method:
                case OperandType.Signature:
                case OperandType.String:
                case OperandType.Token:
                case OperandType.Type:
                    return _reader.ReadBytes(4);

                case OperandType.Float64:
                case OperandType.Int64:
                    return _reader.ReadBytes(8);

                case OperandType.InstructionTable:
                    byte[] header = _reader.ReadBytes(4);
                    byte[] offsets = _reader.ReadBytes(BitConverter.ToInt32(header, 0) * sizeof(int));
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
                        ParameterDefinition paramDef = GetParameter(BitConverter.ToInt16(rawoperand, 0));
                        if (paramDef == null)
                            return BitConverter.ToInt16(rawoperand, 0);
                        return paramDef;

                    case OperandType.ShortArgument:
                        paramDef = GetParameter(rawoperand[0]);
                        if (paramDef == null)
                            return rawoperand[0];
                        return paramDef;

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
                        uint metadata = BitConverter.ToUInt32(rawoperand, 0);
                        try
                        {
                            object operand = TokenResolver.ResolveMember(metadata);

                            if (operand is ISpecification)
                                operand = (operand as ISpecification).TransformWith(MethodBody.Method);

                            return operand;
                        }
                        catch { return new TypeReference(string.Empty, "TOKEN:" + metadata.ToString("X8"), null); }

                    case OperandType.ShortVariable:
                        VariableDefinition varDef = GetVariable(rawoperand[0]);
                        if (varDef == null)
                            return rawoperand[0];
                        return varDef;

                    case OperandType.Variable:
                        varDef = GetVariable(BitConverter.ToInt16(rawoperand, 0));
                        if (varDef == null)
                            return rawoperand[0];
                        return varDef;

                    case OperandType.Signature:
                        return BitConverter.ToInt32(rawoperand, 0);

                    case OperandType.String:
                        return TokenResolver.ResolveString(BitConverter.ToUInt32(rawoperand, 0));
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
            if (MethodBody == null)
                return null;

            if (index >= 0 && index <= MethodBody.Variables.Length)
                return MethodBody.Variables[index];
            return null;
        }

        private ParameterDefinition GetParameter(int index)
        {
            if (MethodBody == null)
                return null;

            if (!MethodBody.Method.Attributes.HasFlag(MethodAttributes.Static))
                index--;
            if (index >= 0 && index <= MethodBody.Method.Parameters.Length)
                return MethodBody.Method.Parameters[index];
            return null;
        }

    }
}
