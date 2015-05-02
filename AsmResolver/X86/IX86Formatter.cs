using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.X86
{
    public interface IX86Formatter
    {
        string FormatMnemonic(X86Mnemonic mnemonic);
        string FormatOperand(X86Operand operand);
    }

    public static class X86FormatterExtensions
    {
        public static string FormatInstruction(this IX86Formatter formatter, X86Instruction instruction)
        {
            var mnemonicString = formatter.FormatMnemonic(instruction.Mnemonic);

            if (instruction.Operand2 == null)
            {
                return instruction.Operand1 == null
                    ? mnemonicString
                    : mnemonicString + ' ' + formatter.FormatOperand(instruction.Operand1);
            }

            return mnemonicString + ' ' + formatter.FormatOperand(instruction.Operand1) + ", " +
                   formatter.FormatOperand(instruction.Operand2);
        }
    }

    public abstract class X86Formatter : IX86Formatter
    {
        public virtual string FormatMnemonic(X86Mnemonic mnemonic)
        {
            return mnemonic.ToString().ToLowerInvariant().Replace('_', ' ');
        }

        public virtual string FormatOperand(X86Operand operand)
        {
            string prefix = FormatOperandTypePrefix(operand.OperandType);

            var formattedValue = FormatValue(operand.Value);
            var formattedCorrection = FormatCorrection(operand.Correction);
            var formattedScaledIndex = operand.ScaledIndex != null ? '+' + operand.ScaledIndex.ToString() : string.Empty;

            return prefix == null
                ? formattedValue
                : string.Format("{0} [{1}{2}{3}]", prefix, formattedValue, formattedScaledIndex, formattedCorrection);
        }

        private string FormatValue(object value)
        {
            if (value is X86Register)
                return value.ToString().ToLowerInvariant();

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return FormatByte((byte)value);
                case TypeCode.UInt16:
                    return FormatWord((ushort)value);
                case TypeCode.UInt32:
                    return FormatDword((uint)value);
                case TypeCode.UInt64:
                    return FormatDword(Convert.ToUInt32(value)); // TODO: qwords
            }
            throw new NotSupportedException();
        }

        public abstract string FormatOperandTypePrefix(X86OperandType operandType);

        public abstract string FormatRegister(X86Register value);

        public abstract string FormatByte(byte value);

        public abstract string FormatWord(ushort value);

        public abstract string FormatDword(uint value);

        public abstract string FormatFword(ulong value);

        public string FormatCorrection(object value)
        {
            if (value == null)
                return string.Empty;
            if (value is sbyte)
                return FormatCorrection((sbyte)value);
            if (value is int)
                return FormatCorrection((int)value);
            throw new NotSupportedException();
        }

        public abstract string FormatCorrection(sbyte value);

        public abstract string FormatCorrection(int value);
    }

    public class FasmX86Formatter : X86Formatter
    {
        public override string FormatOperandTypePrefix(X86OperandType operandType)
        {
            switch (operandType)
            {
                case X86OperandType.Normal:
                    return null;
                case X86OperandType.BytePointer:
                    return "byte";
                case X86OperandType.WordPointer:
                    return "word";
                case X86OperandType.DwordPointer:
                    return "dword";
                case X86OperandType.FwordPointer:
                    return "fword";
            }
            throw new ArgumentException();
        }

        public override string FormatRegister(X86Register value)
        {
            return value.ToString().ToLowerInvariant();
        }

        public override string FormatByte(byte value)
        {
            return "0x" + value.ToString("X");
        }

        public override string FormatWord(ushort value)
        {
            return "0x" + value.ToString("X");
        }

        public override string FormatDword(uint value)
        {
            return "0x" + value.ToString("X");
        }

        public override string FormatFword(ulong value)
        {
            return "0x" + value.ToString("X");
        }

        public override string FormatCorrection(sbyte value)
        {
            if (value == 0)
                return string.Empty;
            return (value < 0 ? '-' : '+') + "0x" + Math.Abs(value).ToString("X");
        }

        public override string FormatCorrection(int value)
        {
            if (value == 0)
                return string.Empty;
            return (value < 0 ? '-' : '+') + "0x" + Math.Abs(value).ToString("X");
        }
    }
}
