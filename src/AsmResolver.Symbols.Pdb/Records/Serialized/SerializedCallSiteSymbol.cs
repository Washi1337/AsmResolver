using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="CallSiteSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedCallSiteSymbol : CallSiteSymbol
{
    private readonly PdbReaderContext _context;
    private readonly uint _typeIndex;

    /// <summary>
    /// Reads a call site symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedCallSiteSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _context = context;
        Offset = reader.ReadInt32();
        SectionIndex = reader.ReadUInt16();
        _ = reader.ReadUInt16(); // padding
        _typeIndex = reader.ReadUInt32();
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetFunctionType()
    {
        return _context.ParentImage.TryGetLeafRecord(_typeIndex, out CodeViewTypeRecord? type)
        ? type
        : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
            $"Call site symbol contains an invalid type index {_typeIndex:X8}.");
    }
}
