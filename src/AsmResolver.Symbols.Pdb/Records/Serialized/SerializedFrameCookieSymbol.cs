using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="FrameCookieSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedFrameCookieSymbol : FrameCookieSymbol
{
    /// <summary>
    /// Reads a frame cookie symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedFrameCookieSymbol(BinaryStreamReader reader)
    {
        FrameOffset = reader.ReadInt32();
        Register = reader.ReadUInt16();
        CookieType = (FrameCookieType) reader.ReadByte();
        Attributes = reader.ReadByte();
    }
}
