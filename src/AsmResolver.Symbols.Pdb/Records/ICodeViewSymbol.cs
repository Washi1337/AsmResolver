namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a single CodeView symbol record within a PDB file.
/// </summary>
public interface ICodeViewSymbol
{
    /// <summary>
    /// Gets the type of symbol this record encodes.
    /// </summary>
    CodeViewSymbolType CodeViewSymbolType
    {
        get;
    }
}
