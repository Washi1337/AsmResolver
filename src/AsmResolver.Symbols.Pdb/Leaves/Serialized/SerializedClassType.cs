using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="ClassType"/> that is read from a PDB image.
/// </summary>
public class SerializedClassType : ClassType
{
    private readonly PdbReaderContext _context;
    private readonly ushort _memberCount;
    private readonly uint _baseTypeIndex;
    private readonly uint _fieldIndex;
    private readonly uint _vshapeIndex;
    private readonly BinaryStreamReader _nameReader;
    private readonly BinaryStreamReader _uniqueNameReader;

    /// <summary>
    /// Reads a class type from the provided input stream.
    /// </summary>
    /// <param name="kind">The kind of type that is being read.</param>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedClassType(CodeViewLeafKind kind, PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(kind, typeIndex)
    {
        _context = context;
        _memberCount = reader.ReadUInt16();
        StructureAttributes = (StructureAttributes) reader.ReadUInt16();
        _fieldIndex = reader.ReadUInt32();
        _baseTypeIndex = reader.ReadUInt32();
        _vshapeIndex = reader.ReadUInt32();

        Size = (uint) ReadNumeric(ref reader);

        _nameReader = reader.Fork();
        reader.AdvanceUntil(0, true);
        _uniqueNameReader = reader.Fork();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override Utf8String GetUniqueName() => _uniqueNameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewType? GetBaseType()
    {
        if (_baseTypeIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_baseTypeIndex, out var leaf) && leaf is CodeViewType type
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewType>(
                $"Class type {TypeIndex:X8} contains an invalid underlying enum type index {_baseTypeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override FieldList? GetFields()
    {
        if (_fieldIndex == 0)
            return null;

        if (!_context.ParentImage.TryGetLeafRecord(_fieldIndex, out var leaf) || leaf is not SerializedFieldList list)
        {
            _context.Parameters.ErrorListener.BadImage(
                $"Class type {TypeIndex:X8} contains an invalid field list index {_fieldIndex:X8}.");
            return new FieldList();
        }

        return list;
    }
}
