using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using TUP.AsmResolver.ASM;

namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// A collection of Assembly Instructions with extra functions.
    /// </summary>
    public class InstructionCollection :  CollectionBase 
    {

        /// <summary>
        /// Gets the assembly instruction in the application by its offset.
        /// </summary>
        /// <param name="TargetOffset">The Offset of the instruction go get.</param>
        /// <returns></returns>
        public x86Instruction GetInstructionByOffset(long TargetOffset)
        {
            x86Instruction TargetInstruction = null;
            foreach (x86Instruction instr in List)
            {
                if (instr.Offset.FileOffset == TargetOffset)
                {
                    TargetInstruction = instr;
                    break;

                }
            }

            return TargetInstruction;
            //int TargetInstructionIndex = assembly.instructions.IndexOf(TargetInstruction);
        }
        /// <summary>
        /// Gets the assembly instruction index by its offset
        /// </summary>
        /// <param name="TargetOffset">The Offset of the instruction go get.</param>
        /// <returns></returns>
        public int GetInstructionIndexByOffset(long TargetOffset)
        {
            x86Instruction TargetInstruction = null;
            foreach (x86Instruction instr in List)
            {
                if (instr.Offset.FileOffset == TargetOffset)
                {
                    TargetInstruction = instr;
                    break;
                }
            }
            if (TargetInstruction == null)
                return -1;

            return List.IndexOf(TargetInstruction);
        }
        /// <summary>
        /// Gets the assembly instruction in the application by its virtual offset.
        /// </summary>
        /// <param name="TargetOffset">The Offset of the instruction go get.</param>
        /// <returns></returns>
        public x86Instruction GetInstructionByVirtualOffset(ulong TargetOffset)
        {
            x86Instruction TargetInstruction = null;
            foreach (x86Instruction instr in List)
            {
                if (instr.Offset.Va == TargetOffset)
                {
                    TargetInstruction = instr;
                    break;

                }
            }

            return TargetInstruction;
        }
        /// <summary>
        /// Gets the assembly instruction in the application by its virtual offset.
        /// </summary>
        /// <param name="TargetOffset">The Offset of the instruction go get.</param>
        /// <returns></returns>
        public int GetInstructionIndexByVirtualOffset(ulong TargetOffset)
        {
            x86Instruction TargetInstruction = null;
            foreach (x86Instruction instr in List)
            {
                if (instr.Offset.Va == TargetOffset)
                {
                    TargetInstruction = instr;
                    break;
                }
            }
            if (TargetInstruction == null)
                return -1;

            return List.IndexOf(TargetInstruction);
        }

        /// <summary>
        /// Gets the last instruction of the collection.
        /// </summary>
        /// <returns></returns>
        public x86Instruction Last()
        {
            return (x86Instruction)List[List.Count - 1];
        }
        /// <summary>
        /// Gets the first instruction of the collection.
        /// </summary>
        /// <returns></returns>
        public x86Instruction First()
        {
            return (x86Instruction)List[0];
        }
        /// <summary>
        /// Adds an instruction to the Assembly Instruction Collection.
        /// </summary>
        /// <param name="instruction">The instruction to add.</param>
        public void Add(x86Instruction instruction)
        {
            List.Add(instruction);
        }
        /// <summary>
        /// Inserts at the specified index an instruction into the collection.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="instruction">The instruction to insert into the collection.</param>
        public void Insert(int index, x86Instruction instruction)
        {
            List.Insert(index, instruction);
        }
        /// <summary>
        /// Gets the index of a given instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the index from.</param>
        /// <returns></returns>
        public int IndexOf(x86Instruction instruction)
        {
            return List.IndexOf(instruction);
        }

        /// <summary>
        /// Returns the instruction list in string format.
        /// </summary>
        /// <returns></returns>
        public string GetAsmCode()
        {
            StringBuilder builder = new StringBuilder();
            foreach (x86Instruction instr in List)
                builder.AppendLine(instr.ToString());
            return builder.ToString();
        }
        
        /// <summary>
        /// Gets or sets the instruction at the specific index.
        /// </summary>
        /// <param name="Index">The index of the instruction to get or set.</param>
        /// <returns></returns>
        public x86Instruction this[int Index]
        {
            get
            {
                return (x86Instruction)List[Index];
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return List.IsReadOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether the System.Collections.IList has a fixed size.
        /// </summary>
        public bool IsFixedSize
        {
            get { return List.IsFixedSize; }
        }
        
    

    }
}
