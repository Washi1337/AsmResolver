using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Types;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="UserDefinedTypeSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedUserDefinedTypeSymbol : UserDefinedTypeSymbol
{
    private readonly uint _typeIndex;
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _nameReader;

    /// <summary>
    /// Reads a user-defined type symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedUserDefinedTypeSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _typeIndex = reader.ReadUInt32();
        _context = context;
        _nameReader = reader;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewType? GetSymbolType()
    {
        return _context.ParentImage.TryGetTypeRecord(_typeIndex, out var type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewType>(
                $"User-defined type contains an invalid type index {_typeIndex:X4}.");
    }
}
