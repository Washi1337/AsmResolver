using System;
using System.Linq;

namespace AsmResolver.X86
{
    /// <summary>
    /// Provides methods for formatting a x86 instruction.
    /// </summary>
    public interface IX86Formatter
    {
        /// <summary>
        /// Formats a mnemonic in a x86 assembly instructions to a readable string.
        /// </summary>
        /// <param name="mnemonic">The mnemonic to format.</param>
        /// <returns>The formatted mnemonic.</returns>
        string FormatMnemonic(X86Mnemonic mnemonic);

        /// <summary>
        /// Formats an operand in a x86 instruction to a readable string.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted operand.</returns>
        string FormatOperand(X86Operand operand);
    }

    public static class X86FormatterExtensions
    {
        /// <summary>
        /// Formats an instruction to a readable string.
        /// </summary>
        /// <param name="formatter">The formatter to use.</param>
        /// <param name="instruction">The isntruction to format.</param>
        /// <returns>The formatted operand.</returns>
        public static string FormatInstruction(this IX86Formatter formatter, X86Instruction instruction)
        {
            var mnemonicString = formatter.FormatMnemonic(instruction.Mnemonic);
            
            var operands = new[] { instruction.Operand1, instruction.Operand2, instruction.Operand3 }
                .TakeWhile(x => x != null).Select(formatter.FormatOperand);
            string operandsString = string.Join(", ", operands);

            return mnemonicString + (string.IsNullOrEmpty(operandsString) ? string.Empty : ' ' + operandsString);
//            
//            if (instruction.Operand2 == null)
//            {
//                return instruction.Operand1 == null
//                    ? mnemonicString
//                    : mnemonicString + ' ' + formatter.FormatOperand(instruction.Operand1);
//            }
//
//            return mnemonicString + ' ' + formatter.FormatOperand(instruction.Operand1) + ", " +
//                   formatter.FormatOperand(instruction.Operand2);
        }
    }

    /// <summary>
    /// Provides a base for a generic <see cref="IX86Formatter"/> implementation.
    /// </summary>
    public abstract class X86Formatter : IX86Formatter
    {
        public virtual string FormatMnemonic(X86Mnemonic mnemonic)
        {
            return mnemonic.ToString().ToLowerInvariant().Replace('_', ' ');
        }

        public virtual string FormatOperand(X86Operand operand)
        {
            if (operand == null)
                return string.Empty;
            string prefix = FormatOperandUsagePrefix(operand.OperandUsage);

            var formattedValue = FormatValue(operand.Value);
            var formattedOffset = FormatOffset(operand.Offset);
            var formattedScaledIndex = operand.ScaledIndex != null ? '+' + operand.ScaledIndex.ToString() : string.Empty;

            return prefix == null
                ? formattedValue
                : string.Format("{0} [{1}{2}{3}]", prefix, formattedValue, formattedScaledIndex, formattedOffset);
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
                    return FormatQword((ulong)value);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Formats a operand usage to a readable string. 
        /// </summary>
        /// <param name="operandUsage">The operand usage to format.</param>
        /// <returns>The formatted operand usage prefix.</returns>
        public abstract string FormatOperandUsagePrefix(X86OperandUsage operandUsage);

        /// <summary>
        /// Formats a x86 register to a readable string.
        /// </summary>
        /// <param name="value">The register to format.</param>
        /// <returns>The formatted register.</returns>
        public abstract string FormatRegister(X86Register value);

        /// <summary>
        /// Formats a byte constant to a readable string.
        /// </summary>
        /// <param name="value">The constant to format.</param>
        /// <returns>The formatted constant.</returns>
        public abstract string FormatByte(byte value);

        /// <summary>
        /// Formats a word constant to a readable string.
        /// </summary>
        /// <param name="value">The constant to format.</param>
        /// <returns>The formatted constant.</returns>
        public abstract string FormatWord(ushort value);

        /// <summary>
        /// Formats a dword constant to a readable string.
        /// </summary>
        /// <param name="value">The constant to format.</param>
        /// <returns>The formatted constant.</returns>
        public abstract string FormatDword(uint value);

        /// <summary>
        /// Formats a fword constant to a readable string.
        /// </summary>
        /// <param name="value">The constant to format.</param>
        /// <returns>The formatted constant.</returns>
        public abstract string FormatFword(ulong value);

        /// <summary>
        /// Formats a qword constant to a readable string.
        /// </summary>
        /// <param name="value">The constant to format.</param>
        /// <returns>The formatted constant.</returns>
        public abstract string FormatQword(ulong value);

        /// <summary>
        /// Formats an offset to a readable string.
        /// </summary>
        /// <param name="value">The constant to format.</param>
        /// <returns>The formatted constant.</returns>
        public abstract string FormatOffset(int value);
    }

    /// <summary>
    /// Provides methods for formatting x86 instructions to a code that can be assembled using the flat assembler.
    /// </summary>
    public class FasmX86Formatter : X86Formatter
    {
        public override string FormatOperandUsagePrefix(X86OperandUsage operandUsage)
        {
            switch (operandUsage)
            {
                case X86OperandUsage.Normal:
                    return null;
                case X86OperandUsage.BytePointer:
                    return "byte";
                case X86OperandUsage.WordPointer:
                    return "word";
                case X86OperandUsage.DwordPointer:
                    return "dword";
                case X86OperandUsage.FwordPointer:
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

        public override string FormatQword(ulong value)
        {
            return "0x" + value.ToString("X");
        }

        public override string FormatOffset(int value)
        {
            if (value == 0)
                return string.Empty;
            return (value < 0 ? '-' : '+') + "0x" + Math.Abs(value).ToString("X");
        }
    }
}
