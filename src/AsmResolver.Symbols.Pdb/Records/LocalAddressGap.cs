using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Defines a gap within a range of addresses a local variable is valid.
/// </summary>
/// <param name="Start">The start offset, relative to the beginning of the range.</param>
/// <param name="Length">The number of bytes the range spans.</param>
public record struct LocalAddressGap(ushort Start, ushort Length) : IWritable
{
    /// <summary>
    /// Gets the size in bytes that a local address range gap structure occupies on the disk.
    /// </summary>
    public const uint Size = sizeof(ushort) + sizeof(ushort);

    /// <summary>
    /// Reads a single local address gap from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The range.</returns>
    public static LocalAddressGap FromReader(ref BinaryStreamReader reader)
    {
        return new LocalAddressGap(
            reader.ReadUInt16(),
            reader.ReadUInt16()
        );
    }

    /// <summary>
    /// Gets the (exclusive) end offset of the range.
    /// </summary>
    public ushort End => (ushort) (Start + Length);

    /// <inheritdoc />
    public uint GetPhysicalSize() => Size;

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer)
    {
        writer.WriteUInt16(Start);
        writer.WriteUInt16(Length);
    }
}
