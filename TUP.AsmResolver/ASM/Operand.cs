using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// Represents an operand of an assembly instruction.
    /// </summary>
    public class Operand
    {
        /// <summary>
        /// Creates a new instance of an assembly operand.
        /// </summary>
        /// <param name="value">The value that's being used.</param>
        public Operand(object value) : this(value, OperandType.Normal, 0) 
        {
        }
        /// <summary>
        /// Creates a new instance of an assembly operand.
        /// </summary>
        /// <param name="value">The value that's being used.</param>
        /// <param name="type">The way the value is being used.</param>
        public Operand(object value, OperandType type)
            : this(value, type, 0)
        {
        }
        /// <summary>
        /// Creates a new instance of an operand with an extra addition value.
        /// </summary>
        /// <param name="value">The value that's being used.</param>
        /// <param name="type">The way the value is being used.</param>
        /// <param name="addition">An addition to the value</param>
        public Operand(object value, OperandType type, int addition)
        {
            Value = value;
            ValueType = type;
            Addition = addition;
        }


        /// <summary>
        /// The value that's being used in the operand.
        /// </summary>
        public object Value
        {
            get;
            set;
        }
        /// <summary>
        /// The way the value is being used.
        /// </summary>
        public OperandType ValueType
        {
            get;
            set;
        }
        /// <summary>
        /// The addition to the value.
        /// </summary>
        public int Addition
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the string representation of the operand.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(false);
        }
        /// <summary>
        /// Returns the string representation of the operand.
        /// </summary>
        /// <param name="virtualString">Indicates that the string value will be the virtual representation of the instruction.</param>
        /// <returns></returns>
        public string ToString(bool virtualString)
        {
            string additionstr = "";
            if (Addition > 0)
                additionstr = "+" + Addition.ToString("X");
            else if (Addition < 0)
                additionstr = "-" + (Addition * -1).ToString("X");

            string valueString = ToAsmString(virtualString);
            switch (ValueType)
            {
                case ASM.OperandType.Normal:
                    return valueString;
                case ASM.OperandType.BytePointer:
                    return "BYTE PTR [" + valueString + additionstr + "]";
                case ASM.OperandType.WordPointer:
                    return "WORD PTR [" + valueString + additionstr + "]";
                case ASM.OperandType.DwordPointer:
                    return "DWORD PTR [" + valueString + additionstr + "]";
                case ASM.OperandType.FwordPointer:
                    return "FWORD PTR [" + valueString + additionstr + "]";
                case ASM.OperandType.QwordPointer:
                    return "QWORD PTR [" + valueString + additionstr + "]";
                case OperandType.LeaRegister:
                    return "[" + valueString + additionstr + "]";
            }
            return valueString;
        }


        private string ToAsmString(bool virtualString)
        {
            if (Value is byte)
                return ((byte)Value).ToString("X2");
            if (Value is sbyte)
                return ((sbyte)Value).ToString("X2");
            if (Value is short)
                return ((short)Value).ToString("X4");
            if (Value is ushort)
                return ((ushort)Value).ToString("X4");
            if (Value is int)
                return ((int)Value).ToString("X8");
            if (Value is uint)
                return ((uint)Value).ToString("X8");
            if (Value is long)
                return ((long)Value).ToString("X16");
            if (Value is ulong)
                return ((ulong)Value).ToString("X16");
            if (Value is Offset)
                return ((Offset)Value).ToString(virtualString);
            if (Value is x86Instruction)
                return ((x86Instruction)Value).ToAsmString(virtualString);
            return Value.ToString();
        }
    }
}
