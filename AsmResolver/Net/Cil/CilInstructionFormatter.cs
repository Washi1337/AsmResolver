using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Provides the default implementation of a CIL instruction formatter.
    /// </summary>
    public class CilInstructionFormatter : ICilInstructionFormatter
    {
        private readonly CilMethodBody _methodBody;

        public CilInstructionFormatter(CilMethodBody methodBody)
        {
            _methodBody = methodBody ?? throw new ArgumentNullException(nameof(methodBody));
        }

        public string FormatInstruction(CilInstruction instruction)
        {
            string minimal = FormatLabel(instruction.Offset) + ": " + FormatOpCode(instruction.OpCode);
            return instruction.Operand == null
                ? minimal
                : minimal + FormatOperand(instruction.OpCode.OperandType, instruction.Operand);
        }

        public virtual string FormatLabel(int offset)
        {
            return "IL_" + offset.ToString("X4");
        }

        public virtual string FormatOpCode(CilOpCode opcode)
        {
            return opcode.Name.ToLowerInvariant();
        }

        public virtual string FormatOperand(CilOperandType operandType, object operand)
        {
            switch (operandType)
            {
                case CilOperandType.InlineNone:
                    return string.Empty;
                
                case CilOperandType.ShortInlineBrTarget:
                case CilOperandType.InlineBrTarget:
                    return FormatBranchTarget(operand);
                
                case CilOperandType.InlineType:
                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineTok:
                    return FormatMember(operand);
                
                case CilOperandType.InlineSig:
                    return FormatSignature(operand);
                
                case CilOperandType.ShortInlineI:
                case CilOperandType.InlineI:
                case CilOperandType.InlineI8:
                    return FormatInteger(operand);
                
                case CilOperandType.InlineR:
                case CilOperandType.ShortInlineR:
                    return FormatFloat(operand);
                
                case CilOperandType.InlineString:
                    return FormatString(operand);
                
                case CilOperandType.InlineSwitch:
                    return FormatSwitch(operand);
                
                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                    return FormatVariable(operand);
                
                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                    return FormatArgument(operand);
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(operandType), operandType, null);
            }
        }

        protected virtual string FormatArgument(object operand)
        {
            switch (operand)
            {
                case ParameterSignature signature:
                    int index = _methodBody.Method.Signature.Parameters.IndexOf(signature);
                    if (index != -1)
                    {
                        var paramDef = _methodBody.Method.Parameters.FirstOrDefault(x => x.Sequence == index);
                        return paramDef != null ? paramDef.Name : "A_" + index;
                    }
                    break;
                
                case short longIndex:
                    return "A_" + longIndex;
                
                case byte shortIndex:
                    return "A_" + shortIndex;
            }
            
            return "<<<INVALID>>>";
        }

        protected virtual string FormatVariable(object operand)
        {
            switch (operand)
            {
                case VariableSignature signature:
                    var localVarSig = _methodBody.Signature?.Signature as LocalVariableSignature;
                    int index = localVarSig?.Variables.IndexOf(signature) ?? -1;
                    return index == -1 ? "<<<INVALID>>>" : "V_" + index;

                case short longIndex:
                    return "V_" + longIndex;
                
                case byte shortIndex:
                    return "V_" + shortIndex;

                default:
                    return "<<<INVALID>>>"; 
            }
        }

        protected virtual string FormatInteger(object operand)
        {
            return Convert.ToString(operand, CultureInfo.InvariantCulture);
        }

        protected virtual string FormatFloat(object operand)
        {
            return Convert.ToString(operand, CultureInfo.InvariantCulture);
        }
        protected virtual string FormatSignature(object operand)
        {
            switch (operand)
            {
                case StandAloneSignature signature:
                    return signature.Signature.ToString();
                
                case MetadataToken token:
                    return FormatToken(token);
            }

            return "<<<INVALID>>>";
        }

        private static string FormatToken(MetadataToken token)
        {
            return $"TOKEN<0x{token}>";
        }

        protected virtual string FormatSwitch(object operand)
        {
            switch (operand)
            {
                case IList<CilInstruction> target:
                    return string.Join(", ", target.Select(FormatBranchTarget));
                
                case IList<int> offsets:
                    return string.Join(", ", offsets.Select(x => FormatBranchTarget(x)));
                
                default:
                    return "<<<INVALID>>>";
            }
        }

        protected virtual string FormatString(object operand)
        {
            switch (operand)
            {
                case string value:
                    return '"' + value + '"'; // TODO: escaping
                case MetadataToken token:
                    return FormatToken(token);
                default:
                    return "<<<INVALID>>>";
            }
        }

        protected virtual string FormatBranchTarget(object operand)
        {
            switch (operand)
            {
                case CilInstruction target:
                    return FormatLabel(target.Offset);
                case int offset:
                    return FormatLabel(offset);
                default:
                    return "<<<INVALID>>>";
            }
        }

        protected virtual string FormatMember(object operand)
        {
            switch (operand)
            {
                case IMetadataMember member:
                    return member.ToString();
                
                case MetadataToken token:
                    return FormatToken(token);
                
                default:
                    return "<<<INVALID>>>";
            }
        }
    }
}