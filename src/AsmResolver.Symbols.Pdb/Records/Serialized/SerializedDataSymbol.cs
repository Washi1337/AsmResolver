using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="DataSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedDataSymbol : DataSymbol
{
    private readonly PdbReaderContext _context;
    private readonly uint _typeIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a data symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    /// <param name="isGlobal">Indicates the symbol is a global data symbol.</param>
    public SerializedDataSymbol(PdbReaderContext context, BinaryStreamReader reader, bool isGlobal)
    {
        _context = context;

        _typeIndex = reader.ReadUInt32();
        Offset = reader.ReadUInt32();
        SegmentIndex = reader.ReadUInt16();
        IsGlobal = isGlobal;

        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetVariableType()
    {
        return _context.ParentImage.TryGetLeafRecord(_typeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Data symbol contains an invalid type index {_typeIndex:X8}.");
    }
}
