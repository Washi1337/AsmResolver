using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="ThunkSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedThunkSymbol : ThunkSymbol
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReaderState _nameReaderState;

    /// <summary>
    /// Reads a thunk symbol from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the symbol is situated in.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedThunkSymbol(PdbReaderContext context, BinaryStreamReader reader)
    {
        _context = context;

        _ = reader.ReadUInt32(); // pParent
        _ = reader.ReadUInt32(); // pEnd
        _ = reader.ReadUInt32(); // pNext

        Offset = reader.ReadUInt32();
        SegmentIndex = reader.ReadUInt16();
        Size = reader.ReadUInt16();
        Ordinal = (ThunkOrdinal) reader.ReadByte();

        _nameReaderState = reader.GetState();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReaderState.CreateReader().ReadUtf8String();

}
