using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Records.Serialized;

/// <summary>
/// Represents a lazily initialized implementation of <see cref="ProcedureReferenceSymbol"/> that is read from a PDB image.
/// </summary>
public class SerializedProcedureReferenceSymbol : ProcedureReferenceSymbol
{
    private readonly BinaryStreamReaderState _nameReaderState;

    /// <summary>
    /// Reads a public symbol from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream to read from.</param>
    /// <param name="local">If true, this represents a local procedure reference.</param>
    public SerializedProcedureReferenceSymbol(BinaryStreamReader reader, bool local) : base(local)
    {
        Checksum = reader.ReadUInt32();
        Offset = reader.ReadUInt32();
        Module = reader.ReadUInt16();
        _nameReaderState = reader.GetState();
    }

    /// <inheritdoc />
    protected override Utf8String GetName() => _nameReaderState.CreateReader().ReadUtf8String();
}
