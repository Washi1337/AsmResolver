using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.X86
{
    public class X86Instruction
    {
        private static FasmX86Formatter _formatter = new FasmX86Formatter();

        public X86Instruction()
        {
        }

        internal X86Instruction(long offset)
        {
            Offset = offset;
        }

        public long Offset
        {
            get;
            set;
        }

        public X86OpCode OpCode
        {
            get;
            set;
        }

        public X86Mnemonic Mnemonic
        {
            get;
            set;
        }

        public X86Operand Operand1
        {
            get;
            set;
        }

        public X86Operand Operand2
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0:X8}: {1}", Offset, _formatter.FormatInstruction(this));
        }
    }
}
