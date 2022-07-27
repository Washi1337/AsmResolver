using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

internal class SerializedFieldList : FieldList
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _reader;

    /// <inheritdoc />
    public SerializedFieldList(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _reader = reader;
        reader.RelativeOffset = reader.Length;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.FieldList;

    protected override IList<CodeViewField> GetFields()
    {
        var reader = _reader.Fork();
        var result = new List<CodeViewField>();

        while (reader.CanRead(sizeof(ushort)))
        {
            // Padding
            while (true)
            {
                if ((CodeViewLeafKind) reader.ReadByte() < CodeViewLeafKind.Pad0)
                {
                    reader.Offset--;
                    break;
                }
            }

            var leaf = FromReaderNoHeader(_context, 0, ref reader);
            if (leaf is CodeViewField field)
                result.Add(field);
            else
                _context.Parameters.ErrorListener.BadImage($"Field list contains a non-field leaf {leaf.LeafKind}.");
        }

        return result;
    }
}
