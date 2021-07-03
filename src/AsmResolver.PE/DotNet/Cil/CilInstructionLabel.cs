using System;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Represents a label that references an instruction by its instruction object in a CIL method body.
    /// </summary>
    public class CilInstructionLabel : ICilLabel
    {
        /// <summary>
        /// Creates a new instruction label thar references no instruction yet.
        /// </summary>
        public CilInstructionLabel()
        {
        }

        /// <summary>
        /// Creates a new instruction label.
        /// </summary>
        /// <param name="instruction">The instruction to reference.</param>
        public CilInstructionLabel(CilInstruction instruction)
        {
            Instruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
        }

        /// <summary>
        /// Gets or sets the referenced instruction.
        /// </summary>
        public CilInstruction? Instruction
        {
            get;
            set;
        }

        /// <inheritdoc />
        public int Offset => Instruction?.Offset ?? -1;

        /// <inheritdoc />
        public bool Equals(ICilLabel? other) => other is not null && Offset == other.Offset;

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as ICilLabel);

        /// <inheritdoc />
        public override int GetHashCode() => Offset;

        /// <inheritdoc />
        public override string ToString() => Instruction is null
            ? "IL_????"
            : $"IL_{Offset:X4}";
    }
}
