namespace AsmResolver.PE.Imports;

/// <summary>
/// Provides members for classifying imported symbols by their kind.
/// </summary>
public interface IImportedSymbolClassifier
{
    /// <summary>
    /// Classifies the provided symbol.
    /// </summary>
    /// <param name="symbol">The symbol to classify.</param>
    /// <returns>The classification.</returns>
    ImportedSymbolType Classify(ImportedSymbol symbol);
}
