using System.Linq;

namespace AsmResolver.X86
{
    /// <summary>
    /// Represents an x86 instruction opcode.
    /// </summary>
    public struct X86OpCode
    {
        internal X86OpCode(X86Mnemonic mnemonic, byte opcode, uint operandsValue, bool hasRegisterToken)
            : this(mnemonic, (uint)(opcode << 8), operandsValue, hasRegisterToken)
        {
        }

        internal X86OpCode(X86Mnemonic mnemonic, uint opcodeValue, uint operandsValue, bool hasRegisterToken)
            : this(new X86Mnemonic[]
            {
                mnemonic
            }, opcodeValue, operandsValue, hasRegisterToken, false)
        {
        }

        internal X86OpCode(X86Mnemonic[] mnemonics, uint opcodeValue, uint operandsValue, bool hasRegisterToken, bool hasOpCodeModifier)
            : this()
        {
            Prefix = (byte)((opcodeValue >> 0x18) & 0xFF);
            TwoBytePrefix = (byte)((opcodeValue >> 0x10) & 0xFF);
            Op1 = (byte)((opcodeValue >> 0x08) & 0xFF);
            Op2 = (byte)(opcodeValue & 0xFF);

            Mnemonics = mnemonics;
            HasRegisterToken = hasRegisterToken;

            var method1 = (X86OperandType)((operandsValue >> 0x18) & 0xFF);
            var size1 = (X86OperandSize)((operandsValue >> 0x10) & 0xFF);
            var method2 = (X86OperandType)((operandsValue >> 0x08) & 0xFF);
            var size2 = (X86OperandSize)(operandsValue & 0xFF);

            OperandTypes1 = Enumerable.Repeat(method1, mnemonics.Length).ToArray();
            OperandTypes2 = Enumerable.Repeat(method2, mnemonics.Length).ToArray();
            OperandSizes1 = Enumerable.Repeat(size1, mnemonics.Length).ToArray();
            OperandSizes2 = Enumerable.Repeat(size2, mnemonics.Length).ToArray();

            HasOpCodeModifier = hasOpCodeModifier;

            if (TwoBytePrefix == 0x0F)
                X86OpCodes.MultiByteOpCodes[Op1] = this;
            else
                X86OpCodes.SingleByteOpCodes[Op1] = this;
        }

        internal X86OpCode(X86Mnemonic[] mnemonics, uint opcodeValue, X86OperandType[] operandTypes1, X86OperandSize[] sizes1,
            X86OperandType[] operandTypes2, X86OperandSize[] sizes2)
            : this()
        {
            Prefix = (byte)((opcodeValue >> 0x18) & 0xFF);
            TwoBytePrefix = (byte)((opcodeValue >> 0x10) & 0xFF);
            Op1 = (byte)((opcodeValue >> 0x08) & 0xFF);
            Op2 = (byte)(opcodeValue & 0xFF);
            Mnemonics = mnemonics;
            OperandTypes1 = operandTypes1;
            OperandTypes2 = operandTypes2;
            OperandSizes1 = sizes1;
            OperandSizes2 = sizes2;

            HasOpCodeModifier = true;
            HasRegisterToken = true;

            if (TwoBytePrefix == 0x0F)
                X86OpCodes.MultiByteOpCodes[Op1] = this;
            else
                X86OpCodes.SingleByteOpCodes[Op1] = this;
        }

        public byte Prefix
        {
            get;
            private set;
        }

        public byte TwoBytePrefix
        {
            get;
            private set;
        }

        public byte Op1
        {
            get;
            private set;
        }

        public byte Op2
        {
            get;
            private set;
        }

        public X86Mnemonic[] Mnemonics
        {
            get;
            private set;
        }

        public bool HasRegisterToken
        {
            get;
            private set;
        }

        public bool HasOpCodeModifier
        {
            get;
            private set;
        }

        public X86OperandType[] OperandTypes1
        {
            get;
            private set;
        }

        public X86OperandSize[] OperandSizes1
        {
            get;
            private set;
        }

        public X86OperandSize[] OperandSizes2
        {
            get;
            private set;
        }

        public X86OperandType[] OperandTypes2
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Join("/", Mnemonics.Select(x => x.ToString().Replace('_', ' ').ToLowerInvariant()));
        }
    }
}