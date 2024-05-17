using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.Collections;
using AsmResolver.PE.Exports;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Relocations;
using AsmResolver.Shims;

namespace AsmResolver.DotNet.Code.Native
{
    /// <summary>
    /// Provides a default implementation for <see cref="INativeSymbolsProvider"/> interface, which collects
    /// instances of <see cref="ImportedSymbol"/>, and removes any duplicated symbols.
    /// </summary>
    public class NativeSymbolsProvider : INativeSymbolsProvider
    {
        private readonly Dictionary<string, IImportedModule> _modules = new();
        private readonly Dictionary<ISegmentReference, BaseRelocation> _relocations = new();

        private readonly Dictionary<uint, ExportedSymbol> _fixedExportedSymbols = new();
        private uint _minExportedOrdinal = uint.MaxValue;
        private uint _maxExportedOrdinal = 0;
        private readonly List<ExportedSymbol> _floatingExportedSymbols = new();

        /// <inheritdoc />
        public ISymbol ImportSymbol(ISymbol symbol) => symbol is ImportedSymbol importedSymbol
            ? GetImportedSymbol(importedSymbol)
            : symbol;

        private ImportedSymbol GetImportedSymbol(ImportedSymbol symbol)
        {
            if (symbol.DeclaringModule is null)
                throw new ArgumentException($"Symbol {symbol} is not added to a module.");

            // Find declaring module.
            string? moduleName = symbol.DeclaringModule.Name;
            if (moduleName is null)
                throw new ArgumentException($"Parent module for symbol {symbol} has no name.");
            var module = GetModuleByName(moduleName);

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
                _modules.Add(module.Name!, module);
            }

            return module;
        }

        private static bool TryGetSimilarSymbol(
            ImportedSymbol symbol,
            IImportedModule module,
            [NotNullWhen(true)] out ImportedSymbol? existingSymbol)
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

        /// <inheritdoc />
        public void RegisterExportedSymbol(ExportedSymbol symbol) => RegisterExportedSymbol(symbol, null);

        /// <inheritdoc />
        public void RegisterExportedSymbol(ExportedSymbol symbol, uint? newOrdinal)
        {
            if (!newOrdinal.HasValue)
            {
                _floatingExportedSymbols.Add(symbol);
                return;
            }

            uint ordinal = newOrdinal.Value;

            if (_fixedExportedSymbols.ContainsKey(ordinal))
                throw new ArgumentException($"Ordinal {ordinal.ToString()} was already claimed by another exported symbol.");

            _fixedExportedSymbols.Add(ordinal, symbol);
            _minExportedOrdinal = Math.Min(ordinal, _minExportedOrdinal);
            _maxExportedOrdinal = Math.Max(ordinal, _maxExportedOrdinal);
        }

        /// <summary>
        /// Gets a collection of all imported external modules.
        /// </summary>
        /// <returns>The modules.</returns>
        public IEnumerable<IImportedModule> GetImportedModules() => _modules.Values;

        /// <summary>
        /// Gets a collection of all base relocations that need to be applied in the final PE image.
        /// </summary>
        /// <returns>The base relocations.</returns>
        public IEnumerable<BaseRelocation> GetBaseRelocations() => _relocations.Values;

        /// <summary>
        /// Gets a collection of all symbols that need to be exported in the final PE image.
        /// </summary>
        /// <returns>The exported symbols.</returns>
        public IEnumerable<ExportedSymbol> GetExportedSymbols(out uint baseOrdinal)
        {
            baseOrdinal = 1;
            if (_fixedExportedSymbols.Count == 0 && _floatingExportedSymbols.Count == 0)
                return ArrayShim.Empty<ExportedSymbol>();

            // Check if no fixed symbols at all, then just return the floating exports.
            if (_fixedExportedSymbols.Count == 0)
                return _floatingExportedSymbols;

            // Pre-allocate a buffer of symbols.
            baseOrdinal = _minExportedOrdinal;
            var result = new ExportedSymbol?[_maxExportedOrdinal - _minExportedOrdinal + 1].ToList();

            // Put in all the symbols with fixed ordinals.
            foreach (var fixedEntry in _fixedExportedSymbols)
                result[(int) (fixedEntry.Key - _minExportedOrdinal)] = fixedEntry.Value;

            int slotIndex = (int) _minExportedOrdinal;

            // Prefer filling in gaps for the floating exported symbols.
            int floatingIndex = 0;
            for (; floatingIndex < _floatingExportedSymbols.Count && slotIndex < result.Count; floatingIndex++)
            {
                var floatingEntry = _floatingExportedSymbols[floatingIndex];
                while (slotIndex < result.Count && result[slotIndex] is not null)
                    slotIndex++;
                if (slotIndex < result.Count)
                    result[slotIndex] = floatingEntry;
            }

            // Are there still floating symbols left?
            if (floatingIndex != _floatingExportedSymbols.Count)
            {
                // Prefer inserting the remainder before first ordinal, and just decrease base ordinal.
                int insertBeforeCount = Math.Min((int) _minExportedOrdinal - 1,
                    _floatingExportedSymbols.Count - floatingIndex);
                if (insertBeforeCount > 0)
                {
                    baseOrdinal -= (uint) insertBeforeCount;
                    result.InsertRange(0, new ExportedSymbol?[insertBeforeCount]);

                    slotIndex = 0;
                    for (; floatingIndex < insertBeforeCount; floatingIndex++)
                        result[slotIndex++] = _floatingExportedSymbols[floatingIndex];
                }

                // If we still have anything left, add them to the end.
                for (; floatingIndex < _floatingExportedSymbols.Count; floatingIndex++)
                    result.Add(_floatingExportedSymbols[floatingIndex]);
            }

            // We might still have gaps in the table, fill those up with dummy symbols.
            for (slotIndex = 0; slotIndex < result.Count; slotIndex++)
                result[slotIndex] ??= new ExportedSymbol(SegmentReference.Null);

            return result!;
        }
    }
}
