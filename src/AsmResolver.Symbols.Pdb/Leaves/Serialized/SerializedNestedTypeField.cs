using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="NestedTypeField"/> that is read from a PDB image.
/// </summary>
public class SerializedNestedTypeField : NestedTypeField
{
    private readonly PdbReaderContext _context;
    private readonly uint _typeIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a nested type field from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the field is situated in.</param>
    /// <param name="typeIndex">The index to assign to the field.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedNestedTypeField(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        Attributes = (CodeViewFieldAttributes) reader.ReadUInt16();
        _typeIndex = reader.ReadUInt32();
        _nameReader = reader.Fork();
        reader.AdvanceUntil(0, true);
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetNestedType()
    {
        return _context.ParentImage.TryGetLeafRecord(_typeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Nested type {TypeIndex:X8} contains an invalid type index {_typeIndex:X8}.");
    }
}
