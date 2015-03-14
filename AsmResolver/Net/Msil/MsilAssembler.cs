using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Msil
{
    public class MsilAssembler
    {
        private readonly IOperandBuilder _builder;
        private readonly IBinaryStreamWriter _writer;

        public MsilAssembler(IOperandBuilder builder, IBinaryStreamWriter writer)
        {
            _builder = builder;
            _writer = writer;
        }

        public void Write(MsilInstruction instruction)
        {
            WriteOpCode(instruction.OpCode);
            WriteOperand(instruction);
        }

        private void WriteOpCode(MsilOpCode opCode)
        {
            if (opCode.Size == 2)
                _writer.WriteByte(opCode.Op1);
            _writer.WriteByte(opCode.Op2);
        }

        private void WriteOperand(MsilInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case MsilOperandType.InlineArgument:
                    _writer.WriteUInt16((ushort)_builder.GetParameterIndex((ParameterSignature)instruction.Operand));
                    break;
                case MsilOperandType.ShortInlineArgument:
                    _writer.WriteByte((byte)_builder.GetParameterIndex((ParameterSignature)instruction.Operand));
                    break;

                case MsilOperandType.InlineVar:
                    _writer.WriteUInt16((ushort)_builder.GetVariableIndex((VariableSignature)instruction.Operand));
                    break;
                case MsilOperandType.ShortInlineVar:
                    _writer.WriteByte((byte)_builder.GetVariableIndex((VariableSignature)instruction.Operand));
                    break;

                case MsilOperandType.ShortInlineI:
                    _writer.WriteSByte((sbyte)instruction.Operand);
                    break;
                case MsilOperandType.InlineI:
                    _writer.WriteInt32((int)instruction.Operand);
                    break;
                case MsilOperandType.InlineI8:
                    _writer.WriteInt64((long)instruction.Operand);
                    break;
                case MsilOperandType.ShortInlineR:
                    _writer.WriteSingle((float)instruction.Operand);
                    break;
                case MsilOperandType.InlineR:
                    _writer.WriteDouble((double)instruction.Operand);
                    break;

                case MsilOperandType.InlineBrTarget:
                    _writer.WriteInt32(((MsilInstruction)instruction.Operand).Offset -
                        (instruction.Offset + instruction.Size));
                    break;

                case MsilOperandType.ShortInlineBrTarget:
                    _writer.WriteSByte((sbyte)(((MsilInstruction)instruction.Operand).Offset - 
                        (instruction.Offset + instruction.Size)));
                    break;

                case MsilOperandType.InlineField:
                case MsilOperandType.InlineMethod:
                case MsilOperandType.InlineSig:
                case MsilOperandType.InlineTok:
                case MsilOperandType.InlineType:
                    var token = _builder.GetMemberToken((MetadataMember)instruction.Operand);
                    if (token.Rid == 0)
                        throw new InvalidOperationException(string.Format("Member {0} has an invalid metadata token.",
                            instruction.Operand));
                    _writer.WriteUInt32(token.ToUInt32());
                    break;

                case MsilOperandType.InlineString:
                    _writer.WriteUInt32(_builder.GetStringOffset((string)instruction.Operand));
                    break;

                case MsilOperandType.InlineSwitch:
                    var targets = (MsilInstruction[])instruction.Operand;
                    _writer.WriteInt32(targets.Length);
                    foreach (var target in targets)
                        _writer.WriteInt32(target.Offset - (instruction.Offset + instruction.Size));
                    break;

                case MsilOperandType.InlineNone:
                    break;
            }
        }
    }
}
