using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Leaves;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="ProcedureSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedProcedureSymbol : ProcedureSymbol
{
    private readonly PdbReaderContext _context;
    private readonly uint _typeIndex;
    private readonly BinaryStreamReader _nameReader;
    private readonly bool _isId;

    /// <summary>
    /// Reads a procedure symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    /// <param name="isGlobal">Indicates the symbol is a global symbol.</param>
    /// <param name="isId">Indicates the symbol references a function ID instead of a procedure type.</param>
    public SerializedProcedureSymbol(PdbReaderContext context, BinaryStreamReader reader, bool isGlobal, bool isId)
    {
        _context = context;

        _ = reader.ReadUInt32(); // pParent
        _ = reader.ReadUInt32(); // pEnd
        _ = reader.ReadUInt32(); // pNext

        Size = reader.ReadUInt32();
        DebugStartOffset = reader.ReadUInt32();
        DebugEndOffset = reader.ReadUInt32();

        _typeIndex = reader.ReadUInt32();

        Offset = reader.ReadUInt32();
        SegmentIndex = reader.ReadUInt16();
        Attributes = (ProcedureAttributes) reader.ReadByte();

        _nameReader = reader;
        IsGlobal = isGlobal;
        _isId = isId;
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReader.Fork().ReadUtf8String();

    /// <inheritdoc />
    protected override CodeViewLeaf? GetFunctionType()
    {
        if (_typeIndex == 0)
            return null;

        if (_isId)
        {
            return !_context.ParentImage.TryGetIdLeafRecord(_typeIndex, out FunctionIdentifier? id)
                ? _context.Parameters.ErrorListener.BadImageAndReturn<FunctionIdentifier>(
                    $"Procedure symbol contains an invalid ID index {_typeIndex:X8}.")
                : id;
        }

        return !_context.ParentImage.TryGetLeafRecord(_typeIndex, out ProcedureTypeRecord? procedure)
            ? _context.Parameters.ErrorListener.BadImageAndReturn<ProcedureTypeRecord>(
                $"Procedure symbol contains an invalid type index {_typeIndex:X8}.")
            : procedure;
    }

}
