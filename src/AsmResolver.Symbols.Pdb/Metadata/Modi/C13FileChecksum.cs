using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a file checksum stored in a CodeView C13 line information section.
/// </summary>
/// <param name="FileNameOffset">The offset to the file name the checksum is computed for.</param>
/// <param name="Type">The type of checksum.</param>
/// <param name="Checksum">The checksum.</param>
public record struct C13FileChecksum(uint FileNameOffset, C13FileChecksumType Type, byte[] Checksum) : IWritable
{
    /// <summary>
    /// The size in bytes of a file checksum header.
    /// </summary>
    public const uint HeaderSize = sizeof(uint) // Offset
            + sizeof(byte) // Length
            + sizeof(C13FileChecksumType) // Type
        ;

    /// <summary>
    /// Reads a single file checksum from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The checksum.</returns>
    public static C13FileChecksum FromReader(ref BinaryStreamReader reader)
    {
        uint offset = reader.ReadUInt32();
        byte checksumLength = reader.ReadByte();
        var checksumType = (C13FileChecksumType) reader.ReadByte();

        byte[] checksum = new byte[checksumLength];
        reader.ReadBytes(checksum, 0, checksum.Length);

        return new C13FileChecksum(offset, checksumType, checksum);
    }

    /// <inheritdoc />
    public uint GetPhysicalSize() => (HeaderSize + (uint) Checksum.Length).Align(4);

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer)
    {
        ulong startOffset = writer.Offset;
        writer.WriteUInt32(FileNameOffset);
        writer.WriteByte((byte) Checksum.Length);
        writer.WriteByte((byte) Type);
        writer.WriteBytes(Checksum);
        writer.AlignRelative(4, startOffset);
    }
}
