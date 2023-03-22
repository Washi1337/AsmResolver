namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol record for which the format is unknown or unsupported.
/// </summary>
public class UnknownSymbol : CodeViewSymbol
{
    /// <summary>
    /// Creates a new unknown symbol record.
    /// </summary>
    /// <param name="codeViewSymbolType">The type of symbol.</param>
    /// <param name="data">The raw data stored in the record.</param>
    public UnknownSymbol(CodeViewSymbolType codeViewSymbolType, byte[] data)
    {
        CodeViewSymbolType = codeViewSymbolType;
        Data = data;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType
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
    public override string ToString() => $"S_{CodeViewSymbolType.ToString().ToUpper()} ({Data.Length.ToString()} bytes)";
}
