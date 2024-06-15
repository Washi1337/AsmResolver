using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="SectionSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedSectionSymbol : SectionSymbol
{
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a section symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedSectionSymbol(BinaryStreamReader reader)
    {
        SectionNumber = reader.ReadUInt16();
        Alignment = (uint) (1 << reader.ReadByte());
        reader.ReadByte(); // reserved
        Rva = reader.ReadUInt32();
        Size = reader.ReadUInt32();
        Attributes = (SectionFlags) reader.ReadUInt32();

        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();
}
