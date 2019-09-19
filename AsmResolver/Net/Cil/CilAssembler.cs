using System;
using System.Collections.Generic;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Provides a mechanism for assembling CIL instructions to their binary representation.
    /// </summary>
    public class CilAssembler
    {
        private readonly IOperandBuilder _builder;
        private readonly IBinaryStreamWriter _writer;

        /// <summary>
        /// Creates a new assembler using an operand builder and an output stream.
        /// </summary>
        /// <param name="builder">The raw operand synthesizer.</param>
        /// <param name="writer">The output stream to write the instructions to.</param>
        public CilAssembler(IOperandBuilder builder, IBinaryStreamWriter writer)
        {
            _builder = builder;
            _writer = writer;
        }

        /// <summary>
        /// Writes a single instruction to the output stream.
        /// </summary>
        /// <param name="instruction">The instruction to write.</param>
        public void Write(CilInstruction instruction)
        {
            WriteOpCode(instruction.OpCode);
            WriteOperand(instruction);
        }

        /// <summary>
        /// Writes the operation code to the output stream.
        /// </summary>
        /// <param name="opCode">The opcode to write.</param>
        private void WriteOpCode(CilOpCode opCode)
        {
            if (opCode.Size == 2)
                _writer.WriteByte(opCode.Op1);
            _writer.WriteByte(opCode.Op2);
        }

        /// <summary>
        /// Writes the operand of the instruction to the output stream. 
        /// </summary>
        /// <param name="instruction"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void WriteOperand(CilInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineArgument:
                    _writer.WriteUInt16((ushort)_builder.GetParameterIndex(instruction.Operand as ParameterSignature));
                    break;
                case CilOperandType.ShortInlineArgument:
                    _writer.WriteByte((byte)_builder.GetParameterIndex(instruction.Operand as ParameterSignature));
                    break;

                case CilOperandType.InlineVar:
                    _writer.WriteUInt16((ushort)_builder.GetVariableIndex(instruction.Operand as VariableSignature));
                    break;
                case CilOperandType.ShortInlineVar:
                    _writer.WriteByte((byte)_builder.GetVariableIndex(instruction.Operand as VariableSignature));
                    break;

                case CilOperandType.ShortInlineI:
                    _writer.WriteSByte((sbyte)instruction.Operand);
                    break;
                case CilOperandType.InlineI:
                    _writer.WriteInt32((int)instruction.Operand);
                    break;
                case CilOperandType.InlineI8:
                    _writer.WriteInt64((long)instruction.Operand);
                    break;
                case CilOperandType.ShortInlineR:
                    _writer.WriteSingle((float)instruction.Operand);
                    break;
                case CilOperandType.InlineR:
                    _writer.WriteDouble((double)instruction.Operand);
                    break;

                case CilOperandType.InlineBrTarget:
                    _writer.WriteInt32(((CilInstruction)instruction.Operand).Offset -
                        (instruction.Offset + instruction.Size));
                    break;

                case CilOperandType.ShortInlineBrTarget:
                    _writer.WriteSByte((sbyte) (((CilInstruction) instruction.Operand).Offset -
                                                (instruction.Offset + instruction.Size)));
                    break;

                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    var token = _builder.GetMetadataToken((IMetadataMember) instruction.Operand);
                    if (token.Rid == 0)
                        throw new InvalidOperationException($"Member {instruction.Operand} has an invalid metadata token.");

                    _writer.WriteUInt32(token.ToUInt32());
                    break;

                case CilOperandType.InlineString:
                    _writer.WriteUInt32(_builder.GetStringOffset((string)instruction.Operand));
                    break;

                case CilOperandType.InlineSwitch:
                    var targets = (IList<CilInstruction>) instruction.Operand;
                    _writer.WriteInt32(targets.Count);
                    foreach (var target in targets)
                        _writer.WriteInt32(target.Offset - (instruction.Offset + instruction.Size));
                    break;

                case CilOperandType.InlineNone:
                    break;
            }
        }
    }
}
