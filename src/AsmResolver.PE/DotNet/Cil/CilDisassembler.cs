using System;
using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides a mechanism for decoding CIL instructions from an input stream.
    /// </summary>
    public class CilDisassembler
    {
        private readonly ICilOperandResolver _operandResolver;
        private BinaryStreamReaderState _readerState;

        /// <summary>
        /// Creates a new CIL disassembler using the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read the code from.</param>
        public CilDisassembler(in BinaryStreamReader reader)
        {
            _readerState = reader.GetState();
            _operandResolver = EmptyOperandResolver.Instance;
        }

        /// <summary>
        /// Creates a new CIL disassembler using the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read the code from.</param>
        /// <param name="operandResolver">The object responsible for resolving operands.</param>
        public CilDisassembler(in BinaryStreamReader reader, ICilOperandResolver operandResolver)
        {
            _readerState = reader.GetState();
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
            List<CilInstruction>? branches = null;
            List<CilInstruction>? switches = null;

            var instructions = new List<CilInstruction>();

            var reader = _readerState.CreateReader();
            while (reader.Offset < reader.StartOffset + reader.Length)
            {
                var instruction = ReadInstruction(ref reader);
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
                        branch.Operand = TryResolveLabel(instructions, (ICilLabel) branch.Operand!);
                }

                if (switches is not null)
                {
                    foreach (var @switch in switches)
                    {
                        var labels = (IList<ICilLabel>) @switch.Operand!;
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
        private CilInstruction ReadInstruction(ref BinaryStreamReader reader)
        {
            int offset = (int) reader.RelativeOffset;
            var code = ReadOpCode(ref reader);
            object? operand = ReadOperand(ref reader, code.OperandType);

            return new CilInstruction(offset, code, operand);
        }

        private CilOpCode ReadOpCode(ref BinaryStreamReader reader)
        {
            byte op = reader.ReadByte();
            return op == 0xFE
                ? CilOpCodes.MultiByteOpCodes[reader.ReadByte()]
                : CilOpCodes.SingleByteOpCodes[op];
        }

        private object? ReadOperand(ref BinaryStreamReader reader, CilOperandType operandType)
        {
            switch (operandType)
            {
                case CilOperandType.InlineNone:
                    return null;

                case CilOperandType.ShortInlineI:
                    return reader.ReadSByte();

                case CilOperandType.ShortInlineBrTarget:
                    return new CilOffsetLabel(reader.ReadSByte() + (int) reader.RelativeOffset);

                case CilOperandType.ShortInlineVar:
                    byte shortLocalIndex = reader.ReadByte();
                    return _operandResolver.ResolveLocalVariable(shortLocalIndex) ?? shortLocalIndex;

                case CilOperandType.ShortInlineArgument:
                    byte shortArgIndex = reader.ReadByte();
                    return _operandResolver.ResolveParameter(shortArgIndex) ?? shortArgIndex;

                case CilOperandType.InlineVar:
                    ushort longLocalIndex = reader.ReadUInt16();
                    return _operandResolver.ResolveLocalVariable(longLocalIndex) ?? longLocalIndex;

                case CilOperandType.InlineArgument:
                    ushort longArgIndex = reader.ReadUInt16();
                    return _operandResolver.ResolveParameter(longArgIndex) ?? longArgIndex;

                case CilOperandType.InlineI:
                    return reader.ReadInt32();

                case CilOperandType.InlineBrTarget:
                    return new CilOffsetLabel(reader.ReadInt32() + (int) reader.RelativeOffset);

                case CilOperandType.ShortInlineR:
                    return reader.ReadSingle();

                case CilOperandType.InlineI8:
                    return reader.ReadInt64();

                case CilOperandType.InlineR:
                    return reader.ReadDouble();

                case CilOperandType.InlineString:
                    var stringToken = new MetadataToken(reader.ReadUInt32());
                    return _operandResolver.ResolveString(stringToken) ?? stringToken;

                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    var memberToken = new MetadataToken(reader.ReadUInt32());
                    return _operandResolver.ResolveMember(memberToken) ?? memberToken;

                case CilOperandType.InlinePhi:
                    throw new NotSupportedException();

                case CilOperandType.InlineSwitch:
                    return ReadSwitchTable(ref reader);

                default:
                    throw new ArgumentOutOfRangeException(nameof(operandType), operandType, null);
            }
        }

        private IList<ICilLabel> ReadSwitchTable(ref BinaryStreamReader reader)
        {
            int count = reader.ReadInt32();
            int nextOffset = (int) reader.RelativeOffset + count * sizeof(int);

            var offsets = new List<ICilLabel>(count);
            for (int i = 0; i < count; i++)
                offsets.Add(new CilOffsetLabel(nextOffset + reader.ReadInt32()));

            return offsets;
        }

    }
}
