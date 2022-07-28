using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="FieldList"/> that is read from a PDB image.
/// </summary>
internal class SerializedFieldList : FieldList
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _reader;

    /// <summary>
    /// Reads a field list from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the list is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the enum type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedFieldList(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _reader = reader;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.FieldList;

    protected override IList<CodeViewField> GetEntries()
    {
        var reader = _reader.Fork();
        var result = new List<CodeViewField>();

        while (reader.CanRead(sizeof(ushort)))
        {
            // Skip padding bytes.
            while (reader.CanRead(sizeof(byte)))
            {
                var b = (CodeViewLeafKind) reader.ReadByte();
                if (b < CodeViewLeafKind.Pad0)
                {
                    reader.Offset--;
                    break;
                }
            }

            if (!reader.CanRead(sizeof(byte)))
                break;

            // Read field.
            var leaf = FromReaderNoHeader(_context, 0, ref reader);
            if (leaf is CodeViewField field)
                result.Add(field);
            else
                _context.Parameters.ErrorListener.BadImage($"Field list contains a non-field leaf {leaf.LeafKind}.");
        }

        return result;
    }
}
