using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.X86
{
    public class X86Prefix
    {
        private readonly X86OperandSize[] _operandSizes;
        private readonly X86OperandType[] _operandTypes;
        private readonly X86Mnemonic[] _mnemonics;

        private X86Prefix(byte prefixByte)
        {
            PrefixByte = prefixByte;
            RegisterPrefix();
        }
        
        internal X86Prefix(byte prefixByte, params X86Mnemonic[] mnemonics)
            : this(prefixByte)
        {
            _mnemonics = mnemonics;
        }
        
        internal X86Prefix(byte prefixByte, params X86OperandType[] operandTypes)
            : this(prefixByte)
        {
            _operandTypes = operandTypes;
        }
        
        internal X86Prefix(byte prefixByte, params X86OperandSize[] operandSizes)
            : this(prefixByte)
        {
            _operandSizes = operandSizes;
        }
        
        public byte PrefixByte
        {
            get;
            private set;
        }

        private void RegisterPrefix()
        {
            IList<X86Prefix> list;
            if (!X86Prefixes.PrefixesByByte.TryGetValue(PrefixByte, out list))
                X86Prefixes.PrefixesByByte[PrefixByte] = list = new List<X86Prefix>();
            list.Add(this);
        }

        public bool CanPrecedeOpCode(X86OpCode opcode)
        {
            if (_mnemonics != null)
                return _mnemonics.Any(x => opcode.Mnemonics.Contains(x));
            if (_operandSizes != null)
                return _operandSizes.Contains(opcode.OperandSize3) || 
                       _operandSizes.Any(x => opcode.OperandSizes1.Contains(x) || opcode.OperandSizes2.Contains(x));
            if (_operandTypes != null)
                return _operandTypes.Contains(opcode.OperandType3) || 
                       _operandTypes.Any(x => opcode.OperandTypes1.Contains(x) || opcode.OperandTypes2.Contains(x));
            return false;
        }
        
        
    }
}