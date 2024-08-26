using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a text column range in a CodeView C13 lines number.
/// </summary>
/// <param name="ColumnStart">The start column.</param>
/// <param name="ColumnEnd">The end column.</param>
public record struct C13Column(ushort ColumnStart, ushort ColumnEnd) : IWritable
{
    /// <summary>
    /// The size in bytes of a single column range.
    /// </summary>
    public const uint Size = 2 * sizeof(ushort);

    /// <summary>
    /// Reads a column range from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The column.</returns>
    public static C13Column FromReader(ref BinaryStreamReader reader) => new(reader.ReadUInt16(), reader.ReadUInt16());

    /// <inheritdoc />
    public uint GetPhysicalSize() => Size;

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer)
    {
        writer.WriteUInt16(ColumnStart);
        writer.WriteUInt16(ColumnEnd);
    }
}
