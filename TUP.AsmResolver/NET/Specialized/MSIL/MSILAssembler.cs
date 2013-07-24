using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MSILAssembler
    {
        private PeImage _image;
        private OffsetConverter _offsetConverter;
        private long _bodyOffset = 0;
        private MetaDataTokenResolver _tokenResolver;
        private MSILDisassembler _disassembler;

        public MSILAssembler(MethodBody methodBody)
        {
            this.MethodBody = methodBody;
            _disassembler = new MSILDisassembler(methodBody);
            _image = methodBody.Method._netheader._assembly.Image;
            _offsetConverter = new OffsetConverter(Section.GetSectionByRva(methodBody.Method._netheader._assembly, methodBody.Method.RVA));
            _bodyOffset = _offsetConverter.RvaToFileOffset(methodBody.Method.RVA) + methodBody.HeaderSize;
            _tokenResolver = methodBody.Method._netheader.TokenResolver;
        }

        public MethodBody MethodBody { get; private set; }

        public void Replace(MSILInstruction targetInstruction, MSILInstruction newInstruction)
        {
            Replace(targetInstruction, newInstruction, false, false);
        }
        public void Replace(MSILInstruction targetInstruction, MSILInstruction newInstruction, bool overwriteWhenLarger)
        {
            Replace(targetInstruction, newInstruction, overwriteWhenLarger, false);
        }

        public void Replace(MSILInstruction targetInstruction, MSILInstruction newInstruction, bool overwriteWhenLarger, bool suppressInvalidReferences)
        {
            int targetOffset = (int)(_bodyOffset + targetInstruction.Offset);

            if(!overwriteWhenLarger && targetInstruction.Size < newInstruction.Size)
                throw new ArgumentException("The size of the new instruction is bigger than the target instruction.", "newInstruction");

            newInstruction.Offset = targetInstruction.Offset;
            GenerateOperandBytes(newInstruction);

            if (!suppressInvalidReferences && newInstruction.Operand is MetaDataMember)
            {
                if (!ValidateReference(newInstruction.Operand as MetaDataMember))
                    throw new ArgumentException(newInstruction.Operand.ToString() + " does not match with the metadata member provided in the target assembly", "newInstruction");
            }

            int totalSize = CalculateSpaceNeeded(targetInstruction, newInstruction);
            int NopsToAdd = totalSize - newInstruction.Size;
            byte[] NOPS = new byte[NopsToAdd];

            _image.SetOffset(targetOffset);
            _image.Writer.Write(newInstruction.OpCode.Bytes);
            if (newInstruction.OperandBytes != null)
                _image.Writer.Write(newInstruction.OperandBytes);

            _image.Writer.Write(NOPS);
        }

        public bool ValidateReference(MetaDataMember member)
        {
            if (member.ToString() == _tokenResolver.ResolveMember(member._metadatatoken).ToString())
                return false;
            return true;
        }

        public int CalculateSpaceNeeded(MSILInstruction targetInstruction, MSILInstruction newInstruction)
        {
            return CalculateSpaceNeeded(targetInstruction, newInstruction.Size);
        }
        public int CalculateSpaceNeeded(MSILInstruction targetInstruction, int newSize)
        {
            if (newSize <= targetInstruction.Size)
                return targetInstruction.Size;

            int sizeNeeded = 0;

            int currentOffset = targetInstruction.Offset;
            while (sizeNeeded < newSize)
            {
                _disassembler.CurrentOffset = currentOffset;
                MSILInstruction currentInstruction = _disassembler.DisassembleNextInstruction();
                sizeNeeded += currentInstruction.Size;
                currentOffset += currentInstruction.Size;
            }
            return sizeNeeded;
        }


        internal void GenerateOperandBytes(MSILInstruction instruction)
        {
            if (instruction.OperandBytes == null)
            {
                switch (instruction.OpCode.OperandType)
                {
                    case OperandType.Argument:
                        if (instruction.Operand is ParameterDefinition)
                            instruction.OperandBytes = BitConverter.GetBytes((instruction.Operand as ParameterDefinition).Sequence);
                        break;
                    case OperandType.ShortArgument:
                        if (instruction.Operand is ParameterDefinition)
                            instruction.OperandBytes = BitConverter.GetBytes((byte)(instruction.Operand as ParameterDefinition).Sequence);
                        break;
                    case OperandType.Variable:
                        if (instruction.Operand is VariableDefinition)
                            instruction.OperandBytes = BitConverter.GetBytes((ushort)(instruction.Operand as VariableDefinition).Index);
                        break;
                    case OperandType.ShortVariable:
                        if (instruction.Operand is VariableDefinition)
                            instruction.OperandBytes = BitConverter.GetBytes((byte)(instruction.Operand as VariableDefinition).Index);
                        break;
                    case OperandType.Field:
                    case OperandType.Token:
                    case OperandType.Method:
                    case OperandType.Type:
                        // TODO: importing metadata Members into tablesheap
                        if (instruction.Operand is MetaDataMember)
                            instruction.OperandBytes = BitConverter.GetBytes((instruction.Operand as MetaDataMember)._metadatatoken);
                        break;
                    case OperandType.Float32:
                        if (instruction.Operand is float)
                            instruction.OperandBytes = BitConverter.GetBytes((float)instruction.Operand);
                        break;
                    case OperandType.Float64:
                        if (instruction.Operand is double)
                            instruction.OperandBytes = BitConverter.GetBytes((double)instruction.Operand);
                        break;
                    case OperandType.Int8:
                        if (instruction.Operand is sbyte)
                            instruction.OperandBytes = new byte[] { byte.Parse(((byte)instruction.Operand).ToString("x2"), System.Globalization.NumberStyles.HexNumber) };
                        break;
                    case OperandType.Signature:
                    case OperandType.Int32:
                        if (instruction.Operand is int)
                            instruction.OperandBytes = BitConverter.GetBytes((int)instruction.Operand);
                        break;
                    case OperandType.Int64:
                        if (instruction.Operand is long)
                            instruction.OperandBytes = BitConverter.GetBytes((long)instruction.Operand);
                        break;
                    case OperandType.InstructionTarget:
                        //BitConverter.ToInt32(rawoperand,0) + instructionOffset + opcode.Bytes.Length + sizeof(int);
                        if (instruction.Operand is MSILInstruction)
                            instruction.OperandBytes = BitConverter.GetBytes((instruction.Operand as MSILInstruction).Offset - sizeof(int) - instruction.OpCode.Bytes.Length - instruction.Offset);
                        break;
                    case OperandType.ShortInstructionTarget:
                        if (instruction.Operand is MSILInstruction)
                            instruction.OperandBytes = new byte[] { byte.Parse(((sbyte)((instruction.Operand as MSILInstruction).Offset - sizeof(sbyte) - instruction.OpCode.Bytes.Length - instruction.Offset)).ToString("x2"), System.Globalization.NumberStyles.HexNumber) };
                        break;
                    case OperandType.String:
                        //TODO: importing strings into stringsheap
                        
                    case OperandType.Phi:
                    case OperandType.InstructionTable:
                        throw new NotSupportedException();

                }
                if (instruction.OperandBytes == null && instruction.Operand != null)
                    throw new ArgumentException("Operand must match with the opcode's operand type (" + instruction.OpCode.OperandType.ToString() + ")");
            }
        }
    }
}
