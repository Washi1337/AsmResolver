using System;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun;

internal static class NativeFormat
{
    internal const int ArrayBlockSize = 16;

    public static uint GetEncodedUnsignedSize(uint value) => value switch
    {
        < 0b01111111 => 1,
        < 0b00111111_111111111 => 2,
        < 0b00011111_111111111_11111111 => 3,
        < 0b00001111_111111111_11111111_11111111 => 4,
        _ => 5
    };

    public static uint DecodeUnsigned(ref BinaryStreamReader reader)
    {
        // Reference: https://github.com/dotnet/runtime/blob/163bc5800b469bcb86d59b77da21965916a0f4ce/src/coreclr/tools/Common/Internal/NativeFormat/NativeFormatReader.Primitives.cs#L66

        uint value = 0;

        uint val = reader.ReadByte();
        if ((val & 1) == 0)
        {
            value = val >> 1;
        }
        else if ((val & 2) == 0)
        {
            value = (val >> 2) | ((uint) reader.ReadByte() << 6);
        }
        else if ((val & 4) == 0)
        {
            value = (val >> 3)
                    | ((uint) reader.ReadByte() << 5)
                    | ((uint) reader.ReadByte() << 13);
        }
        else if ((val & 8) == 0)
        {
            value = (val >> 4)
                    | ((uint) reader.ReadByte() << 4)
                    | ((uint) reader.ReadByte() << 12)
                    | ((uint) reader.ReadByte() << 20);
        }
        else if ((val & 16) == 0)
        {
            value = reader.ReadUInt32();
        }
        else
        {
            throw new BadImageFormatException("Invalid encoding of unsigned integer.");
        }

        return value;
    }

    public static void EncodeUnsigned(IBinaryStreamWriter writer, uint value)
    {
        switch (GetEncodedUnsignedSize(value))
        {
            case 1:
                writer.WriteByte((byte) ((value & 0b01111111) << 1));
                break;
            case 2:
                writer.WriteByte((byte) ((value & 0b00111111) << 2 | 0b01));
                writer.WriteByte((byte) (value >> 6));
                break;
            case 3:
                writer.WriteByte((byte) ((value & 0b00011111) << 3 | 0b011));
                writer.WriteByte((byte) ((value >> 5) & 0xFF));
                writer.WriteByte((byte) ((value >> 13) & 0xFF));
                break;
            case 4:
                writer.WriteByte((byte) ((value & 0b00001111) << 4 | 0b0011));
                writer.WriteByte((byte) ((value >> 4) & 0xFF));
                writer.WriteByte((byte) ((value >> 12) & 0xFF));
                writer.WriteByte((byte) ((value >> 20) & 0xFF));
                break;
            case 5:
                writer.WriteByte(0b00001111);
                writer.WriteUInt32(value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    public static bool TryGetArrayElement(
        BinaryStreamReader reader,
        uint entryIndexSize,
        int index,
        out BinaryStreamReader contents)
    {
        // Reference: https://github.com/dotnet/runtime/blob/5de6ca4ddd04b6fd25b93bf76f115aa209ab12ff/src/coreclr/vm/nativeformatreader.h#L370

        switch (entryIndexSize)
        {
            case 0:
                reader.RelativeOffset = (uint) index / ArrayBlockSize;
                reader.RelativeOffset = reader.ReadByte();
                break;

            case 1:
                reader.RelativeOffset = sizeof(ushort) * ((uint) index / ArrayBlockSize);
                reader.RelativeOffset = reader.ReadUInt16();
                break;

            default:
                reader.RelativeOffset = sizeof(uint) * ((uint) index / ArrayBlockSize);
                reader.RelativeOffset = reader.ReadUInt32();
                break;
        }

        for (uint bit = ArrayBlockSize >> 1; bit > 0; bit >>= 1)
        {
            var lookahead = reader.Fork();

            uint value = DecodeUnsigned(ref lookahead);
            if ((index & bit) != 0)
            {
                if ((value & 2) != 0)
                {
                    reader.RelativeOffset += value >> 2;
                    continue;
                }
            }
            else
            {
                if ((value & 1) != 0)
                {
                    reader.RelativeOffset = lookahead.RelativeOffset;
                    continue;
                }
            }

            // Not found
            if ((value & 3) == 0)
            {
                // Matching special leaf node?
                if ((value >> 2) == (index & (ArrayBlockSize - 1)))
                {
                    reader.RelativeOffset = lookahead.RelativeOffset;
                    break;
                }
            }

            contents = default;
            return false;
        }

        contents = reader;
        return true;
    }
}
