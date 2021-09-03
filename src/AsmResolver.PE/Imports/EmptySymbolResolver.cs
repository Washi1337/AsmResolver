using AsmResolver.PE.Exports;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides an empty implementation for the <see cref="ISymbolResolver"/> interface that always returns <c>null</c>.
    /// </summary>
    public sealed class EmptySymbolResolver : ISymbolResolver
    {
        /// <summary>
        /// Gets the singleton instance for the <see cref="EmptySymbolResolver"/> class.
        /// </summary>
        public static EmptySymbolResolver Instance
        {
            get;
        } = new();

        private EmptySymbolResolver()
        {
        }

        /// <inheritdoc />
        public ExportedSymbol? Resolve(ImportedSymbol symbol) => null;
    }
}
