using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

public class SerializedEnumType : EnumType
{
    private readonly PdbReaderContext _context;
    private readonly ushort _memberCount;
    private readonly uint _underlyingType;
    private readonly uint _fieldIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a constant symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedEnumType(PdbReaderContext context, uint typeIndex, ref BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _memberCount = reader.ReadUInt16();
        StructureAttributes = (StructureAttributes) reader.ReadUInt16();
        _underlyingType = reader.ReadUInt32();
        _fieldIndex = reader.ReadUInt32();
        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewLeaf? GetEnumUnderlyingType()
    {
        return _context.ParentImage.TryGetLeafRecord(_underlyingType, out var type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewLeaf>(
                $"Enum type {TypeIndex:X8} contains an invalid underlying enum type index {_underlyingType:X8}.");
    }

    /// <inheritdoc />
    protected override IList<CodeViewField> GetFields()
    {
        if (!_context.ParentImage.TryGetLeafRecord(_fieldIndex, out var leaf) || leaf is not SerializedFieldList list)
        {
            _context.Parameters.ErrorListener.BadImage(
                $"Enum type {TypeIndex:X8} contains an invalid field list index {_fieldIndex:X8}.");
            return new List<CodeViewField>();
        }

        return list.Fields;
    }
}
