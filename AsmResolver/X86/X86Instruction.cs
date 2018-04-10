using System;
using System.Collections.Generic;

namespace AsmResolver.X86
{
    /// <summary>
    /// Represents an x86 instruction.
    /// </summary>
    public class X86Instruction
    {
        private static readonly FasmX86Formatter _formatter = new FasmX86Formatter();

        public X86Instruction()
        {
            Prefixes = new List<X86Prefix>();
        }

        internal X86Instruction(long offset)
            : this()
        {
            Offset = offset;
        }

        /// <summary>
        /// Gets or sets the offset the instruction is located at. This offset can be relative or absolute.
        /// </summary>
        public long Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the prefix being used by the instruction (if available). 
        /// </summary>
        public IList<X86Prefix> Prefixes
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets or sets the opcode being used by the instruction.
        /// </summary>
        public X86OpCode OpCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the mnemonic of the opcode that is being used by the instruction.
        /// </summary>
        public X86Mnemonic Mnemonic
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the first operand of the instruction.
        /// </summary>
        public X86Operand Operand1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the second operand of the instruction.
        /// </summary>
        public X86Operand Operand2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the third operand of the instruction.
        /// </summary>
        public X86Operand Operand3
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0:X8}: {1}", Offset, _formatter.FormatInstruction(this));
        }

        /// <summary>
        /// Computes the size (in bytes) of the instruction.
        /// </summary>
        /// <returns>The size of the instruction in bytes.</returns>
        public int ComputeSize()
        {
            int size = OpCode.TwoBytePrefix == 0 ? 1 : 2; // TODO: multi-byte opcodes.

            size += OpCode.HasRegisterToken ? 1 : 0;

            if (Operand1 != null)
            {
                int mnemonicIndex = Array.IndexOf(OpCode.Mnemonics, Mnemonic);

                size += GetTotalOperandSize(Prefixes, OpCode.OperandTypes1[mnemonicIndex], OpCode.OperandSizes1[mnemonicIndex], Operand1);
                if (Operand2 != null)
                    size += GetTotalOperandSize(Prefixes, OpCode.OperandTypes2[mnemonicIndex], OpCode.OperandSizes2[mnemonicIndex], Operand2);
                if (Operand3 != null)
                    size += GetTotalOperandSize(Prefixes, OpCode.OperandType3, OpCode.OperandSize3, Operand3);
            }
            
            return size;
        }

        private static int GetTotalOperandSize(ICollection<X86Prefix> prefixes, X86OperandType operandType, X86OperandSize operandSize, X86Operand operand)
        {
            int size = (int)operand.OffsetType;
            switch (operandType)
            {
                case X86OperandType.None:
                case X86OperandType.ControlRegister:
                case X86OperandType.DebugRegister:
                case X86OperandType.StackRegister:
                case X86OperandType.Register:
                case X86OperandType.RegisterCl:
                case X86OperandType.RegisterDx:
                case X86OperandType.RegisterEax:
                case X86OperandType.RegisterAl:
                case X86OperandType.ImmediateOne:
                case X86OperandType.SegmentRegister:
                case X86OperandType.OpCodeRegister:
                    break;

                case X86OperandType.DirectAddress:
                case X86OperandType.MemoryAddress:
                    size += 4;
                    break;

                case X86OperandType.RelativeOffset:
                case X86OperandType.ImmediateData:
                    size += GetSize(prefixes, operandSize);
                    break;

                case X86OperandType.RegisterOrMemoryAddress:
                case X86OperandType.StackRegisterOrMemoryAddress:
                    if ((operand.ScaledIndex != null) ||
                        (operand.OperandUsage != X86OperandUsage.Normal && operand.Value is X86Register &&
                         (X86Register)operand.Value == X86Register.Esp))
                        size += 1;
                    if (!(operand.Value is X86Register))
                        size += 4;
                    break;
            }

            return size;
        }

        private static int GetSize(ICollection<X86Prefix> prefixes, X86OperandSize operandSize)
        {
            switch (operandSize)
            {
                case X86OperandSize.Byte:
                    return 1;
                case X86OperandSize.Word:
                    return 2;
                case X86OperandSize.WordOrDword:
                    return prefixes.Contains(X86Prefixes.OperandSizeOverride) ? 2 : 4;
                case X86OperandSize.Dword:
                    return 4;
                case X86OperandSize.Fword:
                    return 6;
            }
            throw new NotSupportedException();
        }
    }
}
