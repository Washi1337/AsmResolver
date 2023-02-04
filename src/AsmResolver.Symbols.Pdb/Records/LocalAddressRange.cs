using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Defines a range of addresses a local variable is valid.
/// </summary>
/// <param name="Start">The start address of the range.</param>
/// <param name="SectionStart">The section the start address is located in.</param>
/// <param name="Length">The number of bytes the range spans.</param>
public record struct LocalAddressRange(uint Start, ushort SectionStart, ushort Length) : IWritable
{
    /// <summary>
    /// Gets the size in bytes that a local address range structure occupies on the disk.
    /// </summary>
    public const uint EntrySize = sizeof(uint) + sizeof(ushort) + sizeof(ushort);

    /// <summary>
    /// Reads a single local address range from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The range.</returns>
    public static LocalAddressRange FromReader(ref BinaryStreamReader reader)
    {
        return new LocalAddressRange(
            reader.ReadUInt32(),
            reader.ReadUInt16(),
            reader.ReadUInt16()
        );
    }

    /// <summary>
    /// Gets the (exclusive) end address of the range.
    /// </summary>
    public uint End => Start + Length;

    /// <inheritdoc />
    public uint GetPhysicalSize() => EntrySize;

    /// <inheritdoc />
    public void Write(IBinaryStreamWriter writer)
    {
        writer.WriteUInt32(Start);
        writer.WriteUInt16(SectionStart);
        writer.WriteUInt16(Length);
    }
}
