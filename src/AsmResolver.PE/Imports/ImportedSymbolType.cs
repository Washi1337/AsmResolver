namespace AsmResolver.PE.Imports;

/// <summary>
/// Provides members describing the different types of symbols that can be imported into a PE.
/// </summary>
public enum ImportedSymbolType
{
    /// <summary>
    /// Indicates the imported symbol is referencing an external function or procedure.
    /// </summary>
    Function,

    /// <summary>
    /// Indicates the imported symbol is referencing a global variable or other data symbol.
    /// </summary>
    Data
}
