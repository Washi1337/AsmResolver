using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides a mechanism for decoding CIL instructions from an input stream.
    /// </summary>
    public class CilDisassembler
    {
        private readonly IBinaryStreamReader _reader;
        private readonly ICilOperandResolver _operandResolver;
        private int _currentOffset;

        /// <summary>
        /// Creates a new CIL disassembler using the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read the code from.</param>
        public CilDisassembler(IBinaryStreamReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <summary>
        /// Creates a new CIL disassembler using the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read the code from.</param>
        /// <param name="operandResolver">The object responsible for resolving operands.</param>
        public CilDisassembler(IBinaryStreamReader reader, ICilOperandResolver operandResolver)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _operandResolver = operandResolver ?? throw new ArgumentNullException(nameof(operandResolver));
        }

        /// <summary>
        /// Gets or sets a value indicating whether branch targets should be resolved to
        /// <see cref="CilInstructionLabel"/> where possible.
        /// </summary>
        public bool ResolveBranchTargets
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Reads all instructions from the input stream.
        /// </summary>
        /// <returns>The instructions.</returns>
        public IList<CilInstruction> ReadInstructions()
        {
            List<CilInstruction> branches = null;
            List<CilInstruction> switches = null;

            var instructions = new List<CilInstruction>();

            while (_reader.Offset < _reader.StartOffset + _reader.Length)
            {
                var instruction = ReadInstruction();
                instructions.Add(instruction);

                if (ResolveBranchTargets)
                {
                    switch (instruction.OpCode.OperandType)
                    {
                        case CilOperandType.ShortInlineBrTarget:
                        case CilOperandType.InlineBrTarget:
                            branches ??= new List<CilInstruction>();
                            branches.Add(instruction);
                            break;

                        case CilOperandType.InlineSwitch:
                            switches ??= new List<CilInstruction>();
                            switches.Add(instruction);
                            break;
                    }
                }
            }

            if (ResolveBranchTargets)
            {
                if (branches is not null)
                {
                    foreach (var branch in branches)
                        branch.Operand = TryResolveLabel(instructions, (ICilLabel) branch.Operand);
                }

                if (switches is not null)
                {
                    foreach (var @switch in switches)
                    {
                        var labels = (IList<ICilLabel>) @switch.Operand;
                        for (int i = 0; i < labels.Count; i++)
                            labels[i] = TryResolveLabel(instructions, labels[i]);
                    }
                }
            }

            return instructions;
        }

        private static ICilLabel TryResolveLabel(IList<CilInstruction> instructions, ICilLabel label)
        {
            int index = instructions.GetIndexByOffset(label.Offset);
            if (index != -1)
                label = instructions[index].CreateLabel();
            return label;
        }

        /// <summary>
        /// Reads the next instruction from the input stream.
        /// </summary>
        /// <returns>The instruction.</returns>
        private CilInstruction ReadInstruction()
        {
            ulong start = _reader.Offset;

            var code = ReadOpCode();
            var operand = ReadOperand(code.OperandType);
            var result = new CilInstruction(_currentOffset, code, operand);

            _currentOffset += (int) (_reader.Offset - start);

            return result;
        }

        private CilOpCode ReadOpCode()
        {
            byte op = _reader.ReadByte();
            return op == 0xFE
                ? CilOpCodes.MultiByteOpCodes[_reader.ReadByte()]
                : CilOpCodes.SingleByteOpCodes[op];
        }

        private object ReadOperand(CilOperandType operandType)
        {
            switch (operandType)
            {
                case CilOperandType.InlineNone:
                    return null;

                case CilOperandType.ShortInlineI:
                    return _reader.ReadSByte();

                case CilOperandType.ShortInlineBrTarget:
                    return new CilOffsetLabel(_reader.ReadSByte() + (int) (_reader.Offset - _reader.StartOffset));

                case CilOperandType.ShortInlineVar:
                    byte shortLocalIndex = _reader.ReadByte();
                    return _operandResolver?.ResolveLocalVariable(shortLocalIndex) ?? shortLocalIndex;

                case CilOperandType.ShortInlineArgument:
                    byte shortArgIndex = _reader.ReadByte();
                    return _operandResolver?.ResolveParameter(shortArgIndex) ?? shortArgIndex;

                case CilOperandType.InlineVar:
                    ushort longLocalIndex = _reader.ReadUInt16();
                    return _operandResolver?.ResolveLocalVariable(longLocalIndex) ?? longLocalIndex;

                case CilOperandType.InlineArgument:
                    ushort longArgIndex = _reader.ReadUInt16();
                    return _operandResolver?.ResolveParameter(longArgIndex) ?? longArgIndex;

                case CilOperandType.InlineI:
                    return _reader.ReadInt32();

                case CilOperandType.InlineBrTarget:
                    return new CilOffsetLabel(_reader.ReadInt32() + (int) (_reader.Offset - _reader.StartOffset));

                case CilOperandType.ShortInlineR:
                    return _reader.ReadSingle();

                case CilOperandType.InlineI8:
                    return _reader.ReadInt64();

                case CilOperandType.InlineR:
                    return _reader.ReadDouble();

                case CilOperandType.InlineString:
                    var stringToken = new MetadataToken(_reader.ReadUInt32());
                    return _operandResolver?.ResolveString(stringToken) ?? stringToken;

                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    var memberToken = new MetadataToken(_reader.ReadUInt32());
                    return _operandResolver?.ResolveMember(memberToken) ?? memberToken;

                case CilOperandType.InlinePhi:
                    throw new NotSupportedException();

                case CilOperandType.InlineSwitch:
                    return ReadSwitchTable();

                default:
                    throw new ArgumentOutOfRangeException(nameof(operandType), operandType, null);
            }
        }

        private IList<ICilLabel> ReadSwitchTable()
        {
            int count = _reader.ReadInt32();
            int nextOffset = (int) _reader.Offset + count * sizeof(int);

            var offsets = new List<ICilLabel>(count);
            for (int i = 0; i < count; i++)
                offsets.Add(new CilOffsetLabel(nextOffset + _reader.ReadInt32()));

            return offsets;
        }

    }
}
