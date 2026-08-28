using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="MemberFunctionIdentifier"/> that is read from a PDB image.
/// </summary>
public class SerializedMemberFunctionIdentifier : MemberFunctionIdentifier
{
    private readonly PdbReaderContext _context;
    private readonly uint _parentTypeIndex;
    private readonly uint _typeIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a member function identifier from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the identifier is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the identifier.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedMemberFunctionIdentifier(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;

        _parentTypeIndex = reader.ReadUInt32();
        _typeIndex = reader.ReadUInt32();

        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetParentType()
    {
        return _context.ParentImage.TryGetLeafRecord(_parentTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Member function identifier {TypeIndex:X8} contains an invalid parent type index {_parentTypeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetFunctionType()
    {
        return _context.ParentImage.TryGetLeafRecord(_typeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Member function identifier {TypeIndex:X8} contains an invalid function type index {_typeIndex:X8}.");
    }
}
