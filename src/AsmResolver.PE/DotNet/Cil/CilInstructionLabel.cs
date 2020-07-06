using System;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Represents a label that references an instruction by its instruction object in a CIL method body.
    /// </summary>
    public struct CilInstructionLabel : ICilLabel
    {
        /// <summary>
        /// Creates a new instruction label.
        /// </summary>
        /// <param name="instruction">The instruction to reference.</param>
        public CilInstructionLabel(CilInstruction instruction)
        {
            Instruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
        }
        
        /// <summary>
        /// Gets the referenced instruction.
        /// </summary>
        public CilInstruction Instruction
        {
            get;
        }

        /// <inheritdoc />
        public int Offset => Instruction.Offset;

        /// <inheritdoc />
        public override string ToString() => "IL_" + Offset.ToString("X4");
    }
}