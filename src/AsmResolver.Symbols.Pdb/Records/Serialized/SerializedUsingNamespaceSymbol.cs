using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="UserDefinedTypeSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedUsingNamespaceSymbol : UsingNamespaceSymbol
{
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a user-defined type symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedUsingNamespaceSymbol(BinaryStreamReader reader)
    {
        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();
}
