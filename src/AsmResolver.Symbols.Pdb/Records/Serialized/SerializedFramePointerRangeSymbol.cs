using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="FramePointerRangeSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedFramePointerRangeSymbol : FramePointerRangeSymbol
{
    private readonly BinaryStreamReaderState? _gapsReaderState;

    /// <summary>
    /// Reads a frame-pointer def-range from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    /// <param name="isFullScope">Indicates whether the range is implicitly defined by the containing function scope.</param>
    public SerializedFramePointerRangeSymbol(BinaryStreamReader reader, bool isFullScope)
    {
        Offset = reader.ReadInt32();
        IsFullScope = isFullScope;

        if (!isFullScope)
        {
            Range = LocalAddressRange.FromReader(ref reader);
            _gapsReaderState = reader.GetState();
        }
    }

    /// <inheritdoc />
    protected override IList<LocalAddressGap> GetGaps()
    {
        var result = new List<LocalAddressGap>();
        if (_gapsReaderState is null)
            return result;

        var reader = _gapsReaderState.Value.CreateReader();
        while (reader.CanRead(LocalAddressGap.Size))
            result.Add(LocalAddressGap.FromReader(ref reader));

        return result;
    }
}
