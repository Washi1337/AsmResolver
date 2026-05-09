using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="FileStaticSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedFileStaticSymbol : FileStaticSymbol
{
    private readonly uint _typeIndex;
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReaderState _nameReaderState;

    /// <summary>
    /// Reads a file static symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedFileStaticSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _context = context;

        _typeIndex = reader.ReadUInt32();
        ModuleFileNameOffset = reader.ReadUInt32();
        Attributes = (LocalAttributes) reader.ReadUInt16();

        _nameReaderState = reader.GetState();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReaderState.CreateReader().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetVariableType()
    {
        return _context.ParentImage.TryGetLeafRecord(_typeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"File static symbol contains an invalid type index {_typeIndex:X8}.");
    }
}
