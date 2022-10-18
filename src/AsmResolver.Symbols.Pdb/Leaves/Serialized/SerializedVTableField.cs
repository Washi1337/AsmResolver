using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="VTableField"/> that is read from a PDB image.
/// </summary>
public class SerializedVTableField : VTableField
{
    private readonly PdbReaderContext _context;
    private readonly uint _pointerTypeIndex;

    /// <summary>
    /// Reads a virtual function table field from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the field is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the field.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedVTableField(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        reader.ReadUInt16(); // padding
        _pointerTypeIndex = reader.ReadUInt32();
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetPointerType()
    {
        return _context.ParentImage.TryGetLeafRecord(_pointerTypeIndex, out var leaf) && leaf is CodeViewTypeRecord type
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Virtual function table type {TypeIndex:X8} contains an invalid pointer type index {_pointerTypeIndex:X8}.");
    }
}
