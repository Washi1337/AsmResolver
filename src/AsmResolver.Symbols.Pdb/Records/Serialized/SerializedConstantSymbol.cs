using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="ConstantSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedConstantSymbol : ConstantSymbol
{
    private readonly PdbReaderContext _context;
    private readonly uint _typeIndex;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a constant symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedConstantSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _context = context;
        _typeIndex = reader.ReadUInt32();
        Value = reader.ReadUInt16();
        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetConstantType()
    {
        return _context.ParentImage.TryGetLeafRecord(_typeIndex, out var leaf) && leaf is CodeViewTypeRecord type
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"Constant contains an invalid type index {_typeIndex:X8}.");
    }
}
