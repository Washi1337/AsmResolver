using AsmResolver.PE.Relocations;

namespace AsmResolver.DotNet.Code.Native
{
    /// <summary>
    /// Provides members for obtaining references to external symbols.
    /// </summary>
    public interface INativeSymbolsProvider
    {
        /// <summary>
        /// Gets or sets the image base the final PE image is using. 
        /// </summary>
        ulong ImageBase
        {
            get;
        }
        
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
    }
}