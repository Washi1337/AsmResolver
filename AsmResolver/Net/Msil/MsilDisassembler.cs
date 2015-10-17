using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Msil
{
    public class MsilDisassembler 
    {
        private readonly IBinaryStreamReader _reader;
        private readonly IOperandResolver _resolver;

        public MsilDisassembler(IBinaryStreamReader reader, IOperandResolver resolver)
        {
            _reader = reader;
            _resolver = resolver;
        }

        public IList<MsilInstruction> Disassemble()
        {
            var instructions = new List<MsilInstruction>();
            while (_reader.Position < _reader.StartPosition + _reader.Length)
                instructions.Add(ReadNextInstruction());

            foreach (var instruction in instructions)
                ResolveOperand(instructions, instruction);

            return instructions;
        }

        private MsilInstruction ReadNextInstruction()
        {
            var offset = (int)(_reader.Position - _reader.StartPosition);

            var b = _reader.ReadByte();

            var code = b == 0xFE
                ? MsilOpCodes.MultiByteOpCodes[_reader.ReadByte()]
                : MsilOpCodes.SingleByteOpCodes[b];

            var operand = ReadRawOperand(_reader, code.OperandType); 

            return new MsilInstruction(offset, code, operand);
        }

        private static object ReadRawOperand(IBinaryStreamReader reader, MsilOperandType msilOperandType)
        {
            switch (msilOperandType)
            {
                case MsilOperandType.InlineNone:
                    return null;

                case MsilOperandType.InlineArgument:
                case MsilOperandType.InlineVar:
                    return reader.ReadInt16();

                case MsilOperandType.InlineI:
                case MsilOperandType.InlineBrTarget:
                    return reader.ReadInt32();

                case MsilOperandType.ShortInlineArgument:
                case MsilOperandType.ShortInlineVar:
                case MsilOperandType.ShortInlineBrTarget:
                case MsilOperandType.ShortInlineI:
                    return reader.ReadSByte();

                case MsilOperandType.ShortInlineR:
                    return reader.ReadSingle();
                case MsilOperandType.InlineR:
                    return reader.ReadDouble();
                case MsilOperandType.InlineI8:
                    return reader.ReadInt64();

                case MsilOperandType.InlineField :
                case MsilOperandType.InlineMethod :
                case MsilOperandType.InlineSig:
                case MsilOperandType.InlineTok:
                case MsilOperandType.InlineType:
                case MsilOperandType.InlineString:
                    return new MetadataToken(reader.ReadUInt32());

                case MsilOperandType.InlineSwitch:
                    var offsets = new int[reader.ReadUInt32()];
                    for (int i = 0; i < offsets.Length; i++)
                        offsets[i] = reader.ReadInt32();
                    return offsets;
            }
            throw new NotSupportedException();
        }

        private void ResolveOperand(IList<MsilInstruction> instructions, MsilInstruction current)
        {
            var nextOffset = current.Offset + current.Size;

            switch (current.OpCode.OperandType)
            {
                case MsilOperandType.InlineArgument:
                case MsilOperandType.ShortInlineArgument:
                    current.Operand = _resolver.ResolveParameter(Convert.ToInt32(current.Operand));
                    break;

                case MsilOperandType.InlineVar:
                case MsilOperandType.ShortInlineVar:
                    current.Operand = _resolver.ResolveVariable(Convert.ToInt32(current.Operand));
                    break;

                case MsilOperandType.ShortInlineBrTarget:
                case MsilOperandType.InlineBrTarget:
                    var targetOffset = nextOffset + Convert.ToInt32(current.Operand);
                    var targetInstruction = instructions.FirstOrDefault(x => x.Offset == targetOffset);
                    if (targetInstruction != null)
                        current.Operand = targetInstruction;
                    break;

                case MsilOperandType.InlineField:
                case MsilOperandType.InlineMethod:
                case MsilOperandType.InlineSig:
                case MsilOperandType.InlineTok:
                case MsilOperandType.InlineType:
                    var member = _resolver.ResolveMember((MetadataToken)current.Operand);
                    if (member != null)
                        current.Operand = member;
                    break;

                case MsilOperandType.InlineString:
                    current.Operand = _resolver.ResolveString(((MetadataToken)current.Operand).ToUInt32());
                    break;

                case MsilOperandType.InlineSwitch:
                    var targetOffsets = (int[])current.Operand;
                    var targets = new MsilInstruction[targetOffsets.Length];
                    for (int i = 0; i < targetOffsets.Length; i++)
                        targets[i] = instructions.FirstOrDefault(
                            x => x.Offset == nextOffset + targetOffsets[i]);
                    current.Operand = targets;
                    break;
            }
        }
    }
}
