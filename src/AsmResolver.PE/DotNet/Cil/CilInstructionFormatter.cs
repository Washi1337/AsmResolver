using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides the default implementation of the <see cref="ICilInstructionFormatter"/> interface.
    /// </summary>
    public class CilInstructionFormatter : ICilInstructionFormatter
    {
        /// <inheritdoc />
        public string FormatInstruction(CilInstruction instruction)
        {
            string minimal = $"{FormatLabel(instruction.Offset)}: {FormatOpCode(instruction.OpCode)}";
            return instruction.Operand == null
                ? minimal
                : $"{minimal} {FormatOperand(instruction.OpCode.OperandType, instruction.Operand)}";
        }

        /// <summary>
        /// Formats a CIL offset as a label.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The formatted string.</returns>
        public virtual string FormatLabel(int offset) => $"IL_{offset:X4}";

        /// <summary>
        /// Formats an operation code.
        /// </summary>
        /// <param name="opcode">The operation code to format.</param>
        /// <returns>The formatted string.</returns>
        public virtual string FormatOpCode(CilOpCode opcode) => opcode.Mnemonic.ToLowerInvariant();

        /// <summary>
        /// Formats an operand to a human readable string.
        /// </summary>
        /// <param name="operandType">The type of operand to format.</param>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the provided operand type is not valid.</exception>
        public virtual string FormatOperand(CilOperandType operandType, object operand) => operandType switch
        {
            CilOperandType.InlineNone => string.Empty,
            CilOperandType.ShortInlineBrTarget => FormatBranchTarget(operand),
            CilOperandType.InlineBrTarget => FormatBranchTarget(operand),
            CilOperandType.InlineType => FormatMember(operand),
            CilOperandType.InlineField => FormatMember(operand),
            CilOperandType.InlineMethod => FormatMember(operand),
            CilOperandType.InlineTok => FormatMember(operand),
            CilOperandType.InlineSig => FormatSignature(operand),
            CilOperandType.ShortInlineI => FormatInteger(operand),
            CilOperandType.InlineI => FormatInteger(operand),
            CilOperandType.InlineI8 => FormatInteger(operand),
            CilOperandType.InlineR => FormatFloat(operand),
            CilOperandType.ShortInlineR => FormatFloat(operand),
            CilOperandType.InlineString => FormatString(operand),
            CilOperandType.InlineSwitch => FormatSwitch(operand),
            CilOperandType.InlineVar => FormatVariable(operand),
            CilOperandType.ShortInlineVar => FormatVariable(operand),
            CilOperandType.InlineArgument => FormatArgument(operand),
            CilOperandType.ShortInlineArgument => FormatArgument(operand),
            _ => throw new ArgumentOutOfRangeException(nameof(operandType), operandType, null)
        };

        /// <summary>
        /// Formats an operand as an <see cref="CilOperandType.InlineArgument"/> or
        /// <see cref="CilOperandType.ShortInlineArgument"/>.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatArgument(object operand) => operand switch
        {
            short longIndex => $"A_{longIndex.ToString()}",
            byte shortIndex => $"A_{shortIndex.ToString()}",
            null => "<<<INVALID>>>",
            _ => operand.ToString()
        };

        /// <summary>
        /// Formats an operand as an <see cref="CilOperandType.InlineVar"/> or
        /// <see cref="CilOperandType.ShortInlineVar"/>.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatVariable(object operand) => operand switch
        {
            short longIndex => $"V_{longIndex.ToString()}",
            byte shortIndex => $"V_{shortIndex.ToString()}",
            null => "<<<INVALID>>>",
            _ => operand.ToString()
        };

        /// <summary>
        /// Formats an integer operand.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatInteger(object operand) =>
            Convert.ToString(operand, CultureInfo.InvariantCulture);


        /// <summary>
        /// Formats an integer operand.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatFloat(object operand) =>
            Convert.ToString(operand, CultureInfo.InvariantCulture);

        /// <summary>
        /// Formats a reference to a stand-alone signature.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatSignature(object operand) => operand switch
        {
            MetadataToken token => FormatToken(token),
            null => "<<<INVALID>>>",
            _ => operand.ToString()
        };

        /// <summary>
        /// Formats a raw metadata token.
        /// </summary>
        /// <param name="token">The token to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatToken(MetadataToken token) => $"TOKEN<0x{token.ToString()}>";

        /// <summary>
        /// Formats a switch table operand.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatSwitch(object operand) => operand switch
        {
            IEnumerable<ICilLabel> target => string.Join(", ", target.Select(FormatBranchTarget)),
            IEnumerable<int> offsets => string.Join(", ", offsets.Select(x => FormatBranchTarget(x))),
            null => "<<<INVALID>>>",
            _ => operand.ToString()
        };

        /// <summary>
        /// Formats a string operand.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatString(object operand) => operand switch
        {
            string value => $"\"{value}\"",
            MetadataToken token => FormatToken(token),
            _ => "<<<INVALID>>>"
        };

        /// <summary>
        /// Formats a branch target operand.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatBranchTarget(object operand) => operand switch
        {
            ICilLabel target => FormatLabel(target.Offset),
            int offset => FormatLabel(offset),
            null => "<<<INVALID>>>",
            _ => operand.ToString()
        };

        /// <summary>
        /// Formats a reference to a member.
        /// </summary>
        /// <param name="operand">The operand to format.</param>
        /// <returns>The formatted string.</returns>
        protected virtual string FormatMember(object operand) => operand switch
        {
            MetadataToken token => FormatToken(token),
            null => "<<<INVALID>>>",
            _ => operand.ToString()
        };
    }
}
