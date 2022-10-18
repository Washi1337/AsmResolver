using AsmResolver.PE.Exports;
using AsmResolver.PE.Relocations;

namespace AsmResolver.DotNet.Code.Native
{
    /// <summary>
    /// Provides members for obtaining references to external symbols.
    /// </summary>
    public interface INativeSymbolsProvider
    {
        /// <summary>
        /// Adds a single symbol to the prototype.
        /// </summary>
        /// <param name="symbol">The symbol to import.</param>
        /// <returns>The imported symbol.</returns>
        ISymbol ImportSymbol(ISymbol symbol);

        /// <summary>
        /// Adds a base relocation to the prototype.
        /// </summary>
        /// <param name="relocation">The relocation.</param>
        void RegisterBaseRelocation(BaseRelocation relocation);

        /// <summary>
        /// Adds an exported symbol to the prototype.
        /// </summary>
        /// <param name="symbol">The symbol to export.</param>
        void RegisterExportedSymbol(ExportedSymbol symbol);

        /// <summary>
        /// Adds an exported symbol to the prototype.
        /// </summary>
        /// <param name="symbol">The symbol to export.</param>
        /// <param name="newOrdinal"></param>
        void RegisterExportedSymbol(ExportedSymbol symbol, uint? newOrdinal);
    }
}
