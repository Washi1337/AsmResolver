using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="CoffGroupSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedCoffGroup : CoffGroupSymbol
{
    private readonly BinaryStreamReaderState _nameReaderState;

    /// <summary>
    /// Reads a COFF group symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedCoffGroup(BinaryStreamReader reader)
    {
        Size = reader.ReadUInt32();
        Characteristics = (SectionFlags) reader.ReadUInt32();
        Offset = reader.ReadUInt32();
        SegmentIndex = reader.ReadUInt16();

        _nameReaderState = reader.GetState();
    }

    /// <inheritdoc />
    protected override Utf8String? GetName() => _nameReaderState.CreateReader().ReadUtf8String();
}
