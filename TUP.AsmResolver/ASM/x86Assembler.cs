using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// A class that is able to edit 32-bit assembly instructions to a <see cref="TUP.AsmResolver.Win32Assembly"/>.  UNDER CONSTRUCTION
    /// </summary>
    public class x86Assembler
    {
        PeImage image;
        x86Disassembler disassembler;
        OffsetConverter offsetConverter;

        internal x86Assembler(Win32Assembly assembly)
        {
            this.image = assembly._peImage;
            disassembler = assembly._disassembler;
            offsetConverter = new OffsetConverter(Section.GetSectionByRva(assembly, assembly._ntHeader.OptionalHeader.BaseOfCode));
        }
        /// <summary>
        /// Replaces an instruction with a new instruction.
        /// </summary>
        /// <param name="targetInstruction">The instruction to replace.</param>
        /// <param name="newInstruction">The new instruction.</param>
        public void Replace(x86Instruction targetInstruction, x86Instruction newInstruction)
        {
            Replace(targetInstruction, newInstruction, false);
        }
        /// <summary>
        /// Replaces an instruction with a new instruction.
        /// </summary>
        /// <param name="targetInstruction">The instruction to replace.</param>
        /// <param name="newInstruction">The new instruction.</param>
        /// <param name="overwriteWhenLarger">A boolean indicating whenever the new instruction is larger, it should overwrite the following instructions.</param>
        public void Replace(x86Instruction targetInstruction, x86Instruction newInstruction, bool overwriteWhenLarger)
        {
            int targetOffset = (int)targetInstruction.Offset.FileOffset;
            if (!overwriteWhenLarger && targetInstruction.Size < newInstruction.Size)
                throw new ArgumentException("The size of the new instruction is bigger than the target instruction.", "newInstruction");

            newInstruction.Offset = targetInstruction.Offset;

            int NopsToAdd = CalculateSpaceNeeded(targetInstruction, newInstruction) - newInstruction.Size;
            byte[] NOPS = new byte[NopsToAdd];

            for (int i = 0; i < NopsToAdd; i++)
                NOPS[i] = 0x90;

            image.SetOffset(targetOffset);
            image.Writer.Write(newInstruction.OpCode._opcodeBytes);
            if (newInstruction.operandbytes != null)
                image.Writer.Write(newInstruction.operandbytes);
            image.Writer.Write(NOPS);
        }
        /// <summary>
        /// Calculates and returns the size in bytes needed when replacing an instruction.
        /// </summary>
        /// <param name="targetInstruction">The instruction to be replaced.</param>
        /// <param name="newInstruction">The new instruction.</param>
        /// <returns></returns>
        public int CalculateSpaceNeeded(x86Instruction targetInstruction, x86Instruction newInstruction)
        {
            return CalculateSpaceNeeded(targetInstruction, newInstruction.Size);
        }
        /// <summary>
        /// Calculates and returns the size in bytes needed when replacing an instruction
        /// </summary>
        /// <param name="targetInstruction">The instruction to be replaced.</param>
        /// <param name="newSize">The new instruction's size.</param>
        /// <returns></returns>
        public int CalculateSpaceNeeded(x86Instruction targetInstruction, int newSize)
        {
            if (newSize <= targetInstruction.Size)
                return targetInstruction.Size;

            int sizeNeeded = 0;

            uint currentOffset = targetInstruction.Offset.FileOffset;
            while (sizeNeeded < newSize)
            {
                disassembler.CurrentOffset = currentOffset;
                x86Instruction currentInstruction = disassembler.DisassembleNextInstruction();
                sizeNeeded += currentInstruction.Size;
                currentOffset += (uint)currentInstruction.Size;
            }
            return sizeNeeded;
        }

    }
}
