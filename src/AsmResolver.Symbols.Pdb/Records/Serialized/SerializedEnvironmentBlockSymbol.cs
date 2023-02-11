using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="EnvironmentBlockSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedEnvironmentBlockSymbol : EnvironmentBlockSymbol
{
    private readonly BinaryStreamReader _entriesReader;

    /// <summary>
    /// Reads a constant symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedEnvironmentBlockSymbol(BinaryStreamReader reader)
    {
        reader.ReadByte(); // padding?
        _entriesReader = reader;
    }

    /// <inheritdoc />
    protected override IList<KeyValuePair<Utf8String, Utf8String>> GetEntries()
    {
        var result = new List<KeyValuePair<Utf8String, Utf8String>>();

        var reader = _entriesReader.Fork();
        while (reader.CanRead(sizeof(byte)) && reader.PeekByte() != 0)
        {
            var key = reader.ReadUtf8String();
            var value = reader.ReadUtf8String();
            result.Add(new KeyValuePair<Utf8String, Utf8String>(key, value));
        }

        return result;
    }
}
