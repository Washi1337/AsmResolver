using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides an implementation of the <see cref="IImportedModule"/> class, which can be instantiated and added
    /// to an existing portable executable image.
    /// </summary>
    public class ImportedModule : IImportedModule
    {
        private IList<ImportedSymbol> _members;

        /// <summary>
        /// Creates a new empty module import.
        /// </summary>
        protected ImportedModule()
        {
        }

        /// <summary>
        /// Creates a new module import.
        /// </summary>
        /// <param name="name">The name of the module to import.</param>
        public ImportedModule(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint ForwarderChain
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IList<ImportedSymbol> Symbols
        {
            get
            {
                if (_members is null)
                    Interlocked.CompareExchange(ref _members, GetSymbols(), null);
                return _members;
            }
        }
        /// <summary>
        /// Reads a single module import entry from an input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns></returns>
        public static IImportedModule FromReader(PEReaderContext context, ref BinaryStreamReader reader)
        {
            var entry = new SerializedImportedModule(context, ref reader);
            return entry.IsEmpty ? null : entry;
        }

        /// <summary>
        /// Obtains the collection of members that were imported.
        /// </summary>
        /// <remarks>
        /// This method is called to initialize the value of <see cref="Symbols" /> property.
        /// </remarks>
        /// <returns>The members list.</returns>
        protected virtual IList<ImportedSymbol> GetSymbols() =>
            new OwnedCollection<IImportedModule, ImportedSymbol>(this);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} ({Symbols.Count} symbols)";
        }

    }
}
