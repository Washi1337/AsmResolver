using System;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

#if !NETSTANDARD2_0
using System.Buffers;
#endif

namespace AsmResolver.PE.DotNet.Cil;

/// <summary>
/// Provides methods for patching references to metadata in raw method bodies efficiently.
/// </summary>
public static class FastCilReassembler
{
    private const int TinyExceptionHandlerSize = 2 * sizeof(byte) + 3 * sizeof(ushort) + sizeof(uint);
    private const int FatExceptionHandlerSize = 6 * sizeof(uint);
    private const uint ExceptionHandlerType = 0;

    /// <summary>
    /// Patches the provided code stream.
    /// </summary>
    /// <param name="reader">The input code stream to patch.</param>
    /// <param name="operandResolver">The operand resolver to use for resolving raw operands.</param>
    /// <param name="writer">The output code stream to write to.</param>
    /// <param name="operandBuilder">The operand builder to use for obtaining the new raw operands after they have been resolved.</param>
    public static void RewriteCode(
        ref BinaryStreamReader reader,
        ICilOperandResolver operandResolver,
        BinaryStreamWriter writer,
        ICilOperandBuilder operandBuilder)
    {
#if NETSTANDARD2_0
        byte[] operandBuffer = new byte[8];
#else
        byte[] operandBuffer = ArrayPool<byte>.Shared.Rent(8);
#endif

        while (reader.CanRead(sizeof(byte)))
        {
            var code = ReadWriteOpCode(ref reader, writer);

            MetadataToken token;
            switch (code.OperandType)
            {
                case CilOperandType.InlineNone:
                    break;

                case CilOperandType.ShortInlineI:
                case CilOperandType.ShortInlineArgument:
                case CilOperandType.ShortInlineBrTarget:
                case CilOperandType.ShortInlineVar:
                    writer.WriteByte(reader.ReadByte());
                    break;

                case CilOperandType.InlineVar:
                case CilOperandType.InlineArgument:
                    reader.ReadBytes(operandBuffer, 0, sizeof(ushort));
                    writer.WriteBytes(operandBuffer, 0, sizeof(ushort));
                    break;

                case CilOperandType.InlineBrTarget:
                case CilOperandType.InlineI:
                case CilOperandType.ShortInlineR:
                    reader.ReadBytes(operandBuffer, 0, sizeof(uint));
                    writer.WriteBytes(operandBuffer, 0, sizeof(uint));
                    break;

                case CilOperandType.InlineI8:
                case CilOperandType.InlineR:
                    reader.ReadBytes(operandBuffer, 0, sizeof(ulong));
                    writer.WriteBytes(operandBuffer, 0, sizeof(ulong));
                    break;

                case CilOperandType.InlineSwitch:
                    int count = reader.ReadInt32();
                    writer.WriteInt32(count);

                    int labelsByteCount = count * sizeof(uint);
                    if (operandBuffer.Length < labelsByteCount)
                    {
#if NETSTANDARD2_0
                        operandBuffer = new byte[labelsByteCount];
#else
                        ArrayPool<byte>.Shared.Return(operandBuffer);
                        operandBuffer = ArrayPool<byte>.Shared.Rent(labelsByteCount);
#endif
                    }

                    reader.ReadBytes(operandBuffer, 0, labelsByteCount);
                    writer.WriteBytes(operandBuffer, 0, labelsByteCount);
                    break;

                case CilOperandType.InlinePhi:
                    throw new NotSupportedException();

                case CilOperandType.InlineField:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                case CilOperandType.InlineMethod:
                    token = reader.ReadUInt32();
                    if (operandResolver.ResolveMember(token) is { } member)
                        token = operandBuilder.GetMemberToken(member);
                    writer.WriteUInt32(token.ToUInt32());
                    break;

                case CilOperandType.InlineString:
                    token = reader.ReadUInt32();
                    if (operandResolver.ResolveString(token) is { } s)
                        token = operandBuilder.GetStringToken(s);
                    writer.WriteUInt32(token.ToUInt32());
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

#if !NETSTANDARD2_0
        ArrayPool<byte>.Shared.Return(operandBuffer);
#endif
    }

    private static CilOpCode ReadWriteOpCode(ref BinaryStreamReader reader, BinaryStreamWriter writer)
    {
        byte op = reader.ReadByte();
        writer.WriteByte(op);

        if (op == 0xFE)
        {
            byte op2 = reader.ReadByte();
            writer.WriteByte(op2);
            return CilOpCodes.MultiByteOpCodes[op2];
        }

        return CilOpCodes.SingleByteOpCodes[op];
    }

    /// <summary>
    /// Patches the provided raw extra section containing exception handlers.
    /// </summary>
    /// <param name="reader">The input extra section data to patch.</param>
    /// <param name="operandResolver">The operand resolver to use for resolving any metadata tokens referenced in the handlers.</param>
    /// <param name="writer">The output code stream to write to.</param>
    /// <param name="operandBuilder">The operand builder to use for obtaining the new metadata tokens after they have been resolved.</param>
    /// <param name="fatFormat"></param>
    public static void RewriteExceptionHandlerSection(
        ref BinaryStreamReader reader,
        ICilOperandResolver operandResolver,
        BinaryStreamWriter writer,
        ICilOperandBuilder operandBuilder,
        bool fatFormat)
    {
        int entrySize = fatFormat
            ? FatExceptionHandlerSize
            : TinyExceptionHandlerSize;

#if NETSTANDARD2_0
        byte[] rawEntry = new byte[entrySize];
#else
        byte[] rawEntry = ArrayPool<byte>.Shared.Rent(entrySize);
#endif

        while (reader.CanRead((uint) entrySize))
        {
            // Read next handler entry.
            reader.ReadBytes(rawEntry, 0, entrySize);

            // Carve out handler type.
            uint handlerType = fatFormat
                ? (uint) (rawEntry[0] | rawEntry[1] << 8 | rawEntry[2] << 16 | rawEntry[3] << 24)
                : (uint) (rawEntry[0] | rawEntry[1] << 8);

            // If the handler references an exception type, then we should update the md token.
            if (handlerType == ExceptionHandlerType)
            {
                uint exceptionToken = (uint) (
                    rawEntry[entrySize - 4]
                    | rawEntry[entrySize - 3] << 8
                    | rawEntry[entrySize - 2] << 16
                    | rawEntry[entrySize - 1] << 24
                );

                if (operandResolver.ResolveMember(exceptionToken) is { } resolved)
                {
                    exceptionToken = operandBuilder.GetMemberToken(resolved).ToUInt32();
                    rawEntry[entrySize - 4] = (byte) ((exceptionToken) & 0xFF);
                    rawEntry[entrySize - 3] = (byte) ((exceptionToken >> 8) & 0xFF);
                    rawEntry[entrySize - 2] = (byte) ((exceptionToken >> 16) & 0xFF);
                    rawEntry[entrySize - 1] = (byte) ((exceptionToken >> 24) & 0xFF);
                }
            }

            // Write back exception handler.
            writer.WriteBytes(rawEntry, 0, entrySize);
        }

#if !NETSTANDARD2_0
        ArrayPool<byte>.Shared.Return(rawEntry);
#endif
    }
}
