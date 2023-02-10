using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="RegisterRelativeRangeSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedRegisterRelativeRangeSymbol : RegisterRelativeRangeSymbol
{
    private readonly BinaryStreamReader _gapsReader;

    /// <summary>
    /// Reads a relative register def-range symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedRegisterRelativeRangeSymbol(BinaryStreamReader reader)
    {
        BaseRegister = reader.ReadUInt16();

        ushort flags = reader.ReadUInt16();
        IsSpilledUdtMember = (flags & 1) != 0;
        ParentOffset = (flags >> 4) & 0b0000_1111_1111_1111;
        Offset = reader.ReadInt32();

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
