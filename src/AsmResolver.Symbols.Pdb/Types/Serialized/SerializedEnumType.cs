using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Types.Serialized;

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
    public SerializedEnumType(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
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
    protected override CodeViewType? GetEnumUnderlyingType()
    {
        return _context.ParentImage.TryGetTypeRecord(_underlyingType, out var type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewType>(
                $"Enum type {TypeIndex:X8} contains an invalid underlying enum type index {_underlyingType:X8}.");
    }
}
