namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a compile symbol that uses version 2 of the file format.
/// </summary>
public class Compile2Symbol : CompileSymbol
{
    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Compile2;
}
