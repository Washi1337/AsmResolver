using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="RegisterRangeSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedRegisterRangeSymbol : RegisterRangeSymbol
{
    private readonly BinaryStreamReader _gapsReader;

    /// <summary>
    /// Reads a register def-range symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedRegisterRangeSymbol(BinaryStreamReader reader)
    {
        Register = reader.ReadUInt16();

        ushort flags = reader.ReadUInt16();
        IsMaybe = (flags & 1) != 0;

        Range = LocalAddressRange.FromReader(ref reader);

        _gapsReader = reader;
    }

    /// <inheritdoc />
    protected override IList<LocalAddressGap> GetGaps()
    {
        var result = new List<LocalAddressGap>();

        var reader = _gapsReader.Fork();
        while (reader.CanRead(LocalAddressGap.Size))
            result.Add(LocalAddressGap.FromReader(ref reader));

        return result;
    }
}
