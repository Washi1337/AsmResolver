using System;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides a mechanism for encoding CIL instructions to an output stream. 
    /// </summary>
    public class CilAssembler
    {
        private readonly IBinaryStreamWriter _writer;
        private readonly ICilOperandBuilder _operandBuilder;

        /// <summary>
        /// Creates a new CIL instruction encoder.
        /// </summary>
        /// <param name="writer">The output stream to write the encoded instructions to.</param>
        /// <param name="operandBuilder">The object to use for creating raw operands.</param>
        public CilAssembler(IBinaryStreamWriter writer, ICilOperandBuilder operandBuilder)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _operandBuilder = operandBuilder ?? throw new ArgumentNullException(nameof(operandBuilder));
        }

        /// <summary>
        /// Writes a collection of CIL instructions to the output stream.
        /// </summary>
        /// <param name="instructions">The instructions to write.</param>
        public void WriteInstructions(IEnumerable<CilInstruction> instructions)
        {
            foreach (var instruction in instructions)
                WriteInstruction(instruction);
        }

        /// <summary>
        /// Writes a single instruction to the output stream.
        /// </summary>
        /// <param name="instruction">The instruction to write.</param>
        public void WriteInstruction(CilInstruction instruction)
        {
            WriteOpCode(instruction.OpCode);
            WriteOperand(instruction);
        }

        private void WriteOpCode(CilOpCode opCode)
        {
            _writer.WriteByte(opCode.Byte1);
            if (opCode.IsLarge)
                _writer.WriteByte(opCode.Byte2);
        }

        private void WriteOperand(CilInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineNone:
                    break;
                
                case CilOperandType.ShortInlineI:
                    _writer.WriteSByte((sbyte) instruction.Operand);
                    break;
                case CilOperandType.InlineI:
                    _writer.WriteInt32((int) instruction.Operand);
                    break;
                case CilOperandType.InlineI8:
                    _writer.WriteInt64((long) instruction.Operand);
                    break;
                case CilOperandType.ShortInlineR:
                    _writer.WriteSingle((float) instruction.Operand);
                    break;
                case CilOperandType.InlineR:
                    _writer.WriteDouble((double) instruction.Operand);
                    break;
                
                case CilOperandType.ShortInlineVar:
                    _writer.WriteSByte((sbyte) _operandBuilder.GetVariableIndex(instruction.Operand));
                    break;
                case CilOperandType.InlineVar:
                    _writer.WriteUInt16((ushort) _operandBuilder.GetVariableIndex(instruction.Operand));
                    break;
                case CilOperandType.ShortInlineArgument:
                    _writer.WriteSByte((sbyte) _operandBuilder.GetArgumentIndex(instruction.Operand));
                    break;
                case CilOperandType.InlineArgument:
                    _writer.WriteUInt16((ushort) _operandBuilder.GetArgumentIndex(instruction.Operand));
                    break;

                case CilOperandType.ShortInlineBrTarget:
                    sbyte shortOffset = (sbyte) (((ICilLabel) instruction.Operand).Offset - (int) (_writer.Offset + sizeof(sbyte)));
                    _writer.WriteSByte(shortOffset);
                    break;
                case CilOperandType.InlineBrTarget:
                    int longOffset = ((ICilLabel) instruction.Operand).Offset - (int) (_writer.Offset + sizeof(int));
                    _writer.WriteInt32(longOffset);
                    break;
                
                case CilOperandType.InlineSwitch:
                    var labels = (IList<ICilLabel>) instruction.Operand;
                    _writer.WriteInt32(labels.Count);
                    
                    int baseOffset = (int) _writer.Offset + labels.Count * sizeof(int);
                    for (int i = 0; i < labels.Count; i++)
                        _writer.WriteInt32(labels[i].Offset - baseOffset);

                    break;
                
                case CilOperandType.InlineString:
                    _writer.WriteUInt32(_operandBuilder.GetStringToken(instruction.Operand));
                    break;
                
                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    _writer.WriteUInt32(_operandBuilder.GetMemberToken(instruction.Operand).ToUInt32());
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}