using System;

namespace AsmResolver.PE.Imports;

/// <summary>
/// Implements a <see cref="IImportedSymbolClassifier"/> using a delegate.
/// </summary>
public class DelegatedSymbolClassifier : IImportedSymbolClassifier
{
    private readonly Func<ImportedSymbol, ImportedSymbolType> _classify;

    /// <summary>
    /// Creates a new <see cref="DelegatedSymbolClassifier"/> using the provided delegate.
    /// </summary>
    /// <param name="classify">The classification delegate.</param>
    public DelegatedSymbolClassifier(Func<ImportedSymbol, ImportedSymbolType> classify)
    {
        _classify = classify;
    }

    /// <inheritdoc />
    public ImportedSymbolType Classify(ImportedSymbol symbol) => _classify(symbol);
}
