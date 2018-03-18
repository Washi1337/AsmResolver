using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cil
{
    public class CilDisassembler 
    {
        private readonly IBinaryStreamReader _reader;
        private readonly IOperandResolver _resolver;

        public CilDisassembler(IBinaryStreamReader reader, IOperandResolver resolver)
        {
            _reader = reader;
            _resolver = resolver;
        }

        public IList<CilInstruction> Disassemble()
        {
            var instructions = new List<CilInstruction>();
            while (_reader.Position < _reader.StartPosition + _reader.Length)
                instructions.Add(ReadNextInstruction());

            foreach (var instruction in instructions)
                ResolveOperand(instructions, instruction);

            return instructions;
        }

        private CilInstruction ReadNextInstruction()
        {
            var offset = (int)(_reader.Position - _reader.StartPosition);

            var b = _reader.ReadByte();

            var code = b == 0xFE
                ? CilOpCodes.MultiByteOpCodes[_reader.ReadByte()]
                : CilOpCodes.SingleByteOpCodes[b];

            var operand = ReadRawOperand(_reader, code.OperandType); 

            return new CilInstruction(offset, code, operand);
        }

        private static object ReadRawOperand(IBinaryStreamReader reader, CilOperandType cilOperandType)
        {
            switch (cilOperandType)
            {
                case CilOperandType.InlineNone:
                    return null;

                case CilOperandType.InlineArgument:
                case CilOperandType.InlineVar:
                    return reader.ReadUInt16();

                case CilOperandType.InlineI:
                case CilOperandType.InlineBrTarget:
                    return reader.ReadInt32();

                case CilOperandType.ShortInlineArgument:
                case CilOperandType.ShortInlineVar:
                    return reader.ReadByte();

                case CilOperandType.ShortInlineBrTarget:
                case CilOperandType.ShortInlineI:
                    return reader.ReadSByte();

                case CilOperandType.ShortInlineR:
                    return reader.ReadSingle();
                case CilOperandType.InlineR:
                    return reader.ReadDouble();
                case CilOperandType.InlineI8:
                    return reader.ReadInt64();

                case CilOperandType.InlineField :
                case CilOperandType.InlineMethod :
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                case CilOperandType.InlineString:
                    return new MetadataToken(reader.ReadUInt32());

                case CilOperandType.InlineSwitch:
                    var offsets = new int[reader.ReadUInt32()];
                    for (int i = 0; i < offsets.Length; i++)
                        offsets[i] = reader.ReadInt32();
                    return offsets;
            }
            throw new NotSupportedException();
        }

        private void ResolveOperand(IList<CilInstruction> instructions, CilInstruction current)
        {
            var nextOffset = current.Offset + current.Size;

            switch (current.OpCode.OperandType)
            {
                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                    var parameter = _resolver.ResolveParameter(Convert.ToInt32(current.Operand));
                    if (parameter != null)
                        current.Operand = parameter;
                    break;

                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                    var variable = _resolver.ResolveVariable(Convert.ToInt32(current.Operand));
                    if (variable != null)
                        current.Operand = variable;
                    break;

                case CilOperandType.ShortInlineBrTarget:
                case CilOperandType.InlineBrTarget:
                    var targetInstruction = instructions.FirstOrDefault(
                        x => x.Offset == nextOffset + Convert.ToInt32(current.Operand));
                    if (targetInstruction != null)
                        current.Operand = targetInstruction;
                    break;

                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    var member = _resolver.ResolveMember((MetadataToken)current.Operand);
                    if (member != null)
                        current.Operand = member;
                    break;

                case CilOperandType.InlineString:
                    var stringValue = _resolver.ResolveString(((MetadataToken) current.Operand).ToUInt32());
                    if (stringValue != null)
                        current.Operand = stringValue;
                    break;

                case CilOperandType.InlineSwitch:
                    var targetOffsets = (IList<int>) current.Operand;
                    var targets = new List<CilInstruction>(targetOffsets.Count);
                    for (int i = 0; i < targetOffsets.Count; i++)
                    {
                        targets.Add(instructions.FirstOrDefault(
                            x => x.Offset == nextOffset + targetOffsets[i]));
                    }
                    current.Operand = targets;
                    break;
            }
        }
    }
}
