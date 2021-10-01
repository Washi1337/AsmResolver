using AsmResolver.PE.Exports;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides members for resolving imported symbols.
    /// </summary>
    public interface ISymbolResolver
    {
        /// <summary>
        /// Resolves the provided symbol.
        /// </summary>
        /// <param name="symbol">The symbol to resolve.</param>
        /// <returns>The resolved symbol, or <c>null</c> if none was found.</returns>
        ExportedSymbol? Resolve(ImportedSymbol symbol);
    }
}
