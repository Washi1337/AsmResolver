using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="IndexField"/> that is read from a PDB image.
/// </summary>
public class SerializedIndexField : IndexField
{
    private readonly PdbReaderContext _context;
    private readonly uint _referencedIndex;

    /// <summary>
    /// Reads an index field from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the field is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the field.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedIndexField(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        reader.ReadUInt16(); // padding
        _referencedIndex = reader.ReadUInt32();
    }

    /// <inheritdoc />
    protected override ITpiLeaf? GetReferencedList()
    {
        return _context.ParentImage.TryGetLeafRecord(_referencedIndex, out ITpiLeaf? leaf)
            ? leaf
            : _context.Parameters.ErrorListener.BadImageAndReturn<ITpiLeaf>(
                $"Index field {TypeIndex:X8} contains an invalid continuation index {_referencedIndex:X8}.");
    }
}
