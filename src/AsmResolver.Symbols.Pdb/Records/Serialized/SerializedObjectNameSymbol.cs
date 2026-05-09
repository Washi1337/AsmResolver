using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="ObjectNameSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedObjectNameSymbol : ObjectNameSymbol
{
    private readonly BinaryStreamReaderState _nameReaderState;

    /// <summary>
    /// Reads an object name from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedObjectNameSymbol(BinaryStreamReader reader)
    {
        Signature = reader.ReadUInt32();
        _nameReaderState = reader.GetState();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReaderState.CreateReader().ReadUtf8String();
}
