namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol record for which the format is unknown or unsupported.
/// </summary>
public class UnknownSymbol : SymbolRecord
{
    /// <summary>
    /// Creates a new unknown symbol record.
    /// </summary>
    /// <param name="symbolType">The type of symbol.</param>
    /// <param name="data">The raw data stored in the record.</param>
    public UnknownSymbol(SymbolType symbolType, byte[] data)
    {
        SymbolType = symbolType;
        Data = data;
    }

    /// <inheritdoc />
    public override SymbolType SymbolType
    {
        get;
    }

    /// <summary>
    /// Gets the raw data stored in the record.
    /// </summary>
    public byte[] Data
    {
        get;
    }

    /// <inheritdoc />
    public override string ToString() => $"{SymbolType.ToString()} ({Data.Length.ToString()} bytes)";
}
