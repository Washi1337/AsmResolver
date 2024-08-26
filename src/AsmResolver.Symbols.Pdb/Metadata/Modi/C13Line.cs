using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a text line range in a CodeView C13 lines section.
/// </summary>
/// <param name="Offset">The offset within the section.</param>
/// <param name="LineNumberStart">The first line number the offset is mapped to.</param>
/// <param name="DeltaLineEnd">The difference between the start and end line.</param>
/// <param name="IsStatement">Indicates whether the mapping maps to a statement or an expression.</param>
public record struct C13Line(uint Offset, uint LineNumberStart, byte DeltaLineEnd, bool IsStatement) : IWritable
{
    /// <summary>
    /// The size in bytes of a single C13 line.
    /// </summary>
    public const uint Size = 2 * sizeof(uint);

    /// <summary>
    /// The special line number indicating a line should not be stepped into.
    /// </summary>
    public const uint DoNotStepIntoLineNumber1 = 0xfeefee;

    /// <summary>
    /// The special line number indicating a line should not be stepped into.
    /// </summary>
    public const uint DoNotStepIntoLineNumber2 = 0xf00f00;

    /// <summary>
    /// Reads a single text line range from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The line range.</returns>
    public static C13Line FromReader(ref BinaryStreamReader reader)
    {
        uint offset = reader.ReadUInt32();
        uint rest = reader.ReadUInt32();

        return new C13Line
        {
            Offset = offset,
            LineNumberStart = rest & 0x00FFFFFF,
            DeltaLineEnd = (byte) ((rest >> 24) & 0b01111111),
            IsStatement = (rest & 0x80000000) != 0
        };
    }

    /// <summary>
    /// Indicates the line number is a special line number recognized by the compiler.
    /// </summary>
    public bool IsSpecialLine = LineNumberStart is DoNotStepIntoLineNumber1 or DoNotStepIntoLineNumber2;

    /// <inheritdoc />
    public uint GetPhysicalSize() => Size;

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer)
    {
        writer.WriteUInt32(Offset);
        writer.WriteUInt32(
            (LineNumberStart & 0x00FFFFFF) | ((DeltaLineEnd & 0b01111111u) << 24) | (IsStatement ? 0x80000000u : 0)
        );
    }
}
