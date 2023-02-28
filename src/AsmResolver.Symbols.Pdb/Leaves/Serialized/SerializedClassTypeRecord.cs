using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="ClassTypeRecord"/> that is read from a PDB image.
/// </summary>
public class SerializedClassTypeRecord : ClassTypeRecord
{
    private readonly PdbReaderContext _context;
    private readonly ushort _memberCount;
    private readonly uint _baseTypeIndex;
    private readonly uint _fieldIndex;
    private readonly uint _vTableShapeIndex;
    private readonly BinaryStreamReader _nameReader;
    private readonly BinaryStreamReader _uniqueNameReader;

    /// <summary>
    /// Reads a class type from the provided input stream.
    /// </summary>
    /// <param name="kind">The kind of type that is being read.</param>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedClassTypeRecord(CodeViewLeafKind kind, PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(kind, typeIndex)
    {
        _context = context;
        _memberCount = reader.ReadUInt16();
        StructureAttributes = (StructureAttributes) reader.ReadUInt16();
        _fieldIndex = reader.ReadUInt32();
        _baseTypeIndex = reader.ReadUInt32();
        _vTableShapeIndex = reader.ReadUInt32();

        Size = Convert.ToUInt32(ReadNumeric(ref reader));

        _nameReader = reader.Fork();
        reader.AdvanceUntil(0, true);
        _uniqueNameReader = reader.Fork();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override Utf8String GetUniqueName() => _uniqueNameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetBaseType()
    {
        if (_baseTypeIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_baseTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Class type {TypeIndex:X8} contains an invalid underlying enum type index {_baseTypeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override FieldListLeaf? GetFields()
    {
        if (_fieldIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_fieldIndex, out SerializedFieldListLeaf? list)
            ? list
            : _context.Parameters.ErrorListener.BadImageAndReturn<FieldListLeaf>(
                $"Class type {TypeIndex:X8} contains an invalid field list index {_fieldIndex:X8}.");
    }

    /// <inheritdoc />
    protected override VTableShapeLeaf? GetVTableShape()
    {
        if (_vTableShapeIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_vTableShapeIndex, out VTableShapeLeaf? shape)
            ? shape
            : _context.Parameters.ErrorListener.BadImageAndReturn<VTableShapeLeaf>(
                $"Class type {TypeIndex:X8} contains an invalid VTable shape index {_fieldIndex:X8}.");
    }
}
