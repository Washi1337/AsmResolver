using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="LabelSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedLabelSymbol : LabelSymbol
{
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a label symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedLabelSymbol(BinaryStreamReader reader)
    {
        Offset = reader.ReadUInt32();
        SegmentIndex = reader.ReadUInt16();
        Attributes = (ProcedureAttributes) reader.ReadByte();

        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

}
