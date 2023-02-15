using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="StringIdentifier"/> that is read from a PDB image.
/// </summary>
public class SerializedStringIdentifier : StringIdentifier
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _reader;
    private readonly uint _subStringsIndex;

    /// <summary>
    /// Reads a string ID from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the member is situated in.</param>
    /// <param name="typeIndex">The type index to assign to the member.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedStringIdentifier(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _subStringsIndex = reader.ReadUInt32();
        _reader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetValue() => _reader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override SubStringListLeaf? GetSubStrings()
    {
        if (_subStringsIndex == 0)
            return null;

        return _context.ParentImage.TryGetIdLeafRecord(_subStringsIndex, out SubStringListLeaf? list)
            ? list
            : _context.Parameters.ErrorListener.BadImageAndReturn<SubStringListLeaf>(
                $"String ID {TypeIndex:X8} contains an invalid substrings type index {_subStringsIndex:X8}.");
    }
}
