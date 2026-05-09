using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="PublicSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedPublicSymbol : PublicSymbol
{
    private readonly BinaryStreamReaderState _nameReaderState;

    /// <summary>
    /// Reads a public symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedPublicSymbol(BinaryStreamReader reader)
    {
        Attributes = (PublicSymbolAttributes) reader.ReadUInt32();
        Offset = reader.ReadUInt32();
        SegmentIndex = reader.ReadUInt16();
        _nameReaderState = reader.GetState();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReaderState.CreateReader().ReadUtf8String();
}
