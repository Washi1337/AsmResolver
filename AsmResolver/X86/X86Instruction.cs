using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.X86
{
    public class X86Instruction
    {
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
            var operandString = string.Empty;
            if (Operand1 != null)
            {
                operandString = Operand2 != null ? 
                    string.Format("{0}, {1}", Operand1, Operand2) :
                    Operand1.ToString();
            }
            return string.Format("{0:X8}: {1}{2}", Offset, Mnemonic, operandString);
        }
    }
}
