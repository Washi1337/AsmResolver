using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.X86
{
    /// <summary>
    /// Provides valid offset types for x86 operands.
    /// </summary>
    public enum X86OffsetType
    {
        None = 0,
        Short = 1,
        Long = 4,
    }

    /// <summary>
    /// Represents an operand in an x86 instruction.
    /// </summary>
    public class X86Operand
    {
        internal X86Operand()
        {
        }

        public X86Operand(object value)
            : this(X86OperandUsage.Normal, value)
        {
        }

        public X86Operand(X86OperandUsage operandUsage, object value)
            : this(operandUsage, value, null, 0, X86OffsetType.None)
        {
        }

        public X86Operand(X86OperandUsage operandUsage, object value, int offset)
            : this(operandUsage, value, null, offset, offset >= sbyte.MinValue && offset <= sbyte.MaxValue ? X86OffsetType.Short : X86OffsetType.Long)
        {
        }

        public X86Operand(X86OperandUsage operandUsage, object value, X86ScaledIndex scaledIndex)
            : this(operandUsage, value, scaledIndex, 0, X86OffsetType.None)
        {
        }

        public X86Operand(X86OperandUsage operandUsage, object value, X86ScaledIndex scaledIndex, int offset, X86OffsetType offsetType)
        {
            OperandUsage = operandUsage;
            Value = value;
            ScaledIndex = scaledIndex;
            Offset = offset;
            OffsetType = offsetType;
        }

        /// <summary>
        /// Gets or sets a value indicating how the value of the operand is being used.
        /// </summary>
        public X86OperandUsage OperandUsage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of the operand.
        /// </summary>
        public object Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the scaled index of the operand.
        /// </summary>
        public X86ScaledIndex ScaledIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the offset added to the value.
        /// </summary>
        public int Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the offset.
        /// </summary>
        public X86OffsetType OffsetType
        {
            get;
            set;
        }

        public override string ToString()
        {
            switch (OperandUsage)
            {
                case X86OperandUsage.Normal:
                    return Value.ToString();
                case X86OperandUsage.BytePointer:
                    return string.Format("byte [{0}]", Value);
                case X86OperandUsage.DwordPointer:
                    return string.Format("dword [{0}]", Value);
                case X86OperandUsage.FwordPointer:
                    return string.Format("fword [{0}]", Value);
            }
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Represents a scaled index in an x86 operand.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the register to use.
        /// </summary>
        public X86Register Register
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the constant the value of the register should be multiplied with. Valid values are 1, 2, 4 and 8.
        /// </summary>
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

    /// <summary>
    /// Provides valid usages for x86 operands.
    /// </summary>
    public enum X86OperandUsage
    {
        Normal,
        BytePointer,
        WordPointer,
        DwordPointer,
        FwordPointer,
    }
}
