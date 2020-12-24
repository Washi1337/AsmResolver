using System;
using System.Collections.Generic;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;

namespace AsmResolver.DotNet.Code.Native
{
    /// <summary>
    /// Provides a default implementation for <see cref="INativeSymbolsProvider"/> interface, which collects
    /// instances of <see cref="ImportedSymbol"/>, and removes any duplicated symbols.
    /// </summary>
    public class NativeSymbolsProvider : INativeSymbolsProvider
    {
        private readonly Dictionary<string, IImportedModule> _modules = new Dictionary<string, IImportedModule>();

        private readonly Dictionary<ISegmentReference, BaseRelocation> _relocations =
            new Dictionary<ISegmentReference, BaseRelocation>();

        /// <summary>
        /// Creates a new instance of the <see cref="NativeSymbolsProvider"/> class.
        /// </summary>
        /// <param name="imageBase">The image base of the final PE image.</param>
        public NativeSymbolsProvider(ulong imageBase)
        {
            ImageBase = imageBase;
        }
        
        /// <inheritdoc />
        public ulong ImageBase
        {
            get;
        }

        /// <inheritdoc />
        public ISymbol ImportSymbol(ISymbol symbol)
        {
            if (symbol is ImportedSymbol importedSymbol)
                return GetImportedSymbol(importedSymbol);

            throw new NotSupportedException($"Symbols of type {symbol.GetType()} are not supported.");
        }

        private ImportedSymbol GetImportedSymbol(ImportedSymbol symbol)
        {
            if (symbol.DeclaringModule is null)
                throw new ArgumentException($"Symbol {symbol} is not added to a module.");
         
            // Find declaring module.
            var module = GetModuleByName(symbol.DeclaringModule.Name);
            
            // See if we have already imported this symbol before.
            if (TryGetSimilarSymbol(symbol, module, out var existing))
                return existing;

            // If not, create a copy of the symbol and register it.
            var newSymbol = symbol.IsImportByName
                ? new ImportedSymbol(symbol.Hint, symbol.Name)
                : new ImportedSymbol(symbol.Ordinal);
            module.Symbols.Add(newSymbol);
            return newSymbol;
        }

        private IImportedModule GetModuleByName(string name)
        {
            if (!_modules.TryGetValue(name, out var module))
            {
                module = new ImportedModule(name);
                _modules.Add(module.Name, module);
            }

            return module;
        }

        private static bool TryGetSimilarSymbol(ImportedSymbol symbol, IImportedModule module, out ImportedSymbol existingSymbol)
        {
            for (int i = 0; i < module.Symbols.Count; i++)
            {
                var s = module.Symbols[i];
                if (symbol.IsImportByName != s.IsImportByName)
                    continue;

                if (symbol.IsImportByName)
                {
                    if (symbol.Name == s.Name)
                    {
                        existingSymbol = s;
                        return true;
                    }
                }
                else // if (symbol.IsImportByOrdinal)
                {
                    if (symbol.Ordinal == s.Ordinal)
                    {
                        existingSymbol = s;
                        return true;
                    }
                }
            }

            existingSymbol = null;
            return false;
        }

        /// <inheritdoc />
        public void RegisterBaseRelocation(BaseRelocation relocation)
        {
            if (_relocations.TryGetValue(relocation.Location, out var existing))
            {
                if (existing.Type != relocation.Type)
                    throw new ArgumentException($"Conflicting base relocations for reference {relocation.Location}.");
            }
            else
            {
                _relocations.Add(relocation.Location, relocation);
            }
        }

        /// <summary>
        /// Gets a collection of all imported external modules.
        /// </summary>
        /// <returns>The modules.</returns>
        public IEnumerable<IImportedModule> GetImportedModules() => _modules.Values;

        /// <summary>
        /// Gets a collection of all base relocations that need to be applied in the final PE image.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseRelocation> GetBaseRelocations() => _relocations.Values;
    }
}