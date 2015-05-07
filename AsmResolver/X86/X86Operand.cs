using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.X86
{
    public class X86Operand
    {
        public X86Operand()
        {
        }

        public X86Operand(object value)
        {
            Value = value;
        }

        public X86Operand(object value, X86OperandType operandType)
        {
            Value = value;
            OperandType = operandType;
        }

        public object Value
        {
            get;
            set;
        }

        public X86ScaledIndex ScaledIndex
        {
            get;
            set;
        }

        public object Correction
        {
            get;
            set;
        }

        public X86OperandType OperandType
        {
            get;
            set;
        }

        public override string ToString()
        {
            switch (OperandType)
            {
                case X86OperandType.Normal:
                    return Value.ToString();
                case X86OperandType.BytePointer:
                    return string.Format("byte [{0}]", Value);
                case X86OperandType.DwordPointer:
                    return string.Format("dword [{0}]", Value);
                case X86OperandType.FwordPointer:
                    return string.Format("fword [{0}]", Value);
            }
            throw new NotSupportedException();
        }
    }

    public class X86ScaledIndex
    {
        public X86ScaledIndex()
        {
        }

        public X86ScaledIndex(X86Register register)
            : this(register, 1)
        {
        }

        public X86ScaledIndex(X86Register register, int multiplier)
        {
            Register = register;
            Multiplier = multiplier;
        }

        public X86Register Register
        {
            get;
            set;
        }

        public int Multiplier
        {
            get;
            set;
        }

        public override string ToString()
        {
            if (Multiplier == 1)
                return Register.ToString().ToLowerInvariant();
            return Register.ToString().ToLowerInvariant() + "*" + Multiplier;
        }
    }

    public enum X86OperandType
    {
        Normal,
        BytePointer,
        WordPointer,
        DwordPointer,
        FwordPointer,
    }
}
