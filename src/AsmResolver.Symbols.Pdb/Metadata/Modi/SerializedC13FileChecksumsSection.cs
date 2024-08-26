using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Provides a lazy implementation of <see cref="C13FileChecksumsSection"/> that is read from an input file.
/// </summary>
public class SerializedC13FileChecksumsSection : C13FileChecksumsSection
{
    private readonly BinaryStreamReader _reader;

    /// <summary>
    /// Reads a C13 file checksums section from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    public SerializedC13FileChecksumsSection(BinaryStreamReader reader)
    {
        _reader = reader;
    }

    /// <inheritdoc />
    protected override IList<C13FileChecksum> GetChecksums()
    {
        var result = new List<C13FileChecksum>();

        var reader = _reader;
        while (reader.CanRead(C13FileChecksum.HeaderSize))
        {
            result.Add(C13FileChecksum.FromReader(ref reader));
            reader.AlignRelative(4);
        }

        return result;
    }
}
