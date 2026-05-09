using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="EnvironmentBlockSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedEnvironmentBlockSymbol : EnvironmentBlockSymbol
{
    private readonly BinaryStreamReaderState _entriesReaderState;

    /// <summary>
    /// Reads an environment block symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedEnvironmentBlockSymbol(BinaryStreamReader reader)
    {
        reader.ReadByte(); // padding?
        _entriesReaderState = reader.GetState();
    }

    /// <inheritdoc />
    protected override IList<KeyValuePair<Utf8String, Utf8String>> GetEntries()
    {
        var result = new List<KeyValuePair<Utf8String, Utf8String>>();

        var reader = _entriesReaderState.CreateReader();
        while (reader.CanRead(sizeof(byte)) && reader.PeekByte() != 0)
        {
            var key = reader.ReadUtf8String();
            var value = reader.ReadUtf8String();
            result.Add(new KeyValuePair<Utf8String, Utf8String>(key, value));
        }

        return result;
    }
}
