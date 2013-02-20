using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MSILAssembler
    {
        PeImage image;
        OffsetConverter offsetConverter;
        long bodyOffset = 0;
        MetaDataTokenResolver tokenResolver;
        MSILDisassembler disassembler;

        public MSILAssembler(MethodBody methodBody)
        {
            this.MethodBody = methodBody;
            disassembler = new MSILDisassembler(methodBody);
            image = methodBody.Method.netheader.assembly.Image;
            offsetConverter = new OffsetConverter(Section.GetSectionByRva(methodBody.Method.netheader.assembly, methodBody.Method.RVA));
            bodyOffset = offsetConverter.RvaToFileOffset(methodBody.Method.RVA) + methodBody.HeaderSize;
            tokenResolver = methodBody.Method.netheader.TokenResolver;
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
            int targetOffset = (int)(bodyOffset + targetInstruction.Offset);

            if(!overwriteWhenLarger && targetInstruction.Size < newInstruction.Size)
                throw new ArgumentException("The size of the new instruction is bigger than the target instruction.", "newInstruction");

            newInstruction.Offset = targetInstruction.Offset;
            newInstruction.GenerateBytes();

            if (!suppressInvalidReferences && newInstruction.Operand is MetaDataMember)
            {
                if (!ValidateReference(newInstruction.Operand as MetaDataMember))
                    throw new ArgumentException(newInstruction.Operand.ToString() + " does not match with the metadata member provided in the target assembly", "newInstruction");
            }

            int totalSize = CalculateSpaceNeeded(targetInstruction, newInstruction);
            int NopsToAdd = totalSize - newInstruction.Size;
            byte[] NOPS = new byte[NopsToAdd];

            image.Write(targetOffset, newInstruction.OpCode.Bytes);
            if (newInstruction.OperandBytes != null)
                image.Write(targetOffset + newInstruction.OpCode.Bytes.Length, newInstruction.OperandBytes);

            image.Write(targetOffset + newInstruction.Size, NOPS);
        }

        public bool ValidateReference(MetaDataMember member)
        {
            if (member.ToString() == tokenResolver.ResolveMember(member.metadatatoken).ToString())
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
                disassembler.CurrentOffset = currentOffset;
                MSILInstruction currentInstruction = disassembler.DisassembleNextInstruction();
                sizeNeeded += currentInstruction.Size;
                currentOffset += currentInstruction.Size;
            }
            return sizeNeeded;
        }
    }
}
