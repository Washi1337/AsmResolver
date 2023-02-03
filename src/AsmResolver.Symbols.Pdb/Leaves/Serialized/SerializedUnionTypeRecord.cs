using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="UnionTypeRecord"/> that is read from a PDB image.
/// </summary>
public class SerializedUnionTypeRecord : UnionTypeRecord
{
    private readonly PdbReaderContext _context;
    private readonly ushort _memberCount;
    private readonly uint _fieldIndex;
    private readonly BinaryStreamReader _nameReader;
    private readonly BinaryStreamReader _uniqueNameReader;

    /// <summary>
    /// Reads a union type from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="typeIndex">The index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedUnionTypeRecord(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _memberCount = reader.ReadUInt16();
        StructureAttributes = (StructureAttributes) reader.ReadUInt16();
        _fieldIndex = reader.ReadUInt32();

        Size = Convert.ToUInt64(ReadNumeric(ref reader));

        _nameReader = reader.Fork();
        reader.AdvanceUntil(0, true);
        _uniqueNameReader = reader.Fork();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override Utf8String GetUniqueName() => _uniqueNameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override FieldListLeaf? GetFields()
    {
        if (_fieldIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_fieldIndex, out SerializedFieldListLeaf? list)
            ? list
            : _context.Parameters.ErrorListener.BadImageAndReturn<FieldListLeaf>(
                $"Union type {TypeIndex:X8} contains an invalid field list index {_fieldIndex:X8}.");
    }
}
