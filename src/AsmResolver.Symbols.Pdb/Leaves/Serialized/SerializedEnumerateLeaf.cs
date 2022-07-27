using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

public class SerializedEnumerateLeaf : EnumerateLeaf
{
    private readonly BinaryStreamReader _nameReader;

    public SerializedEnumerateLeaf(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        Attributes = (CodeViewFieldAttributes) reader.ReadUInt16();
        var kind = (CodeViewLeafKind) reader.ReadUInt16();

        // We need to eagerly initialize the value because it is the only way to know how large the leaf is.
        Value = kind switch
        {
            < CodeViewLeafKind.Numeric => (object) (uint) kind,
            CodeViewLeafKind.Char => (char) reader.ReadByte(),
            CodeViewLeafKind.Short => reader.ReadInt16(),
            CodeViewLeafKind.UShort => reader.ReadUInt16(),
            CodeViewLeafKind.Long => reader.ReadInt32(),
            CodeViewLeafKind.ULong => reader.ReadUInt32(),
            CodeViewLeafKind.QuadWord => reader.ReadInt64(),
            CodeViewLeafKind.UQuadWord => reader.ReadUInt64(),
            _ => 0
        };

        _nameReader = reader;
        reader.AdvanceUntil(0, true);
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();
}
