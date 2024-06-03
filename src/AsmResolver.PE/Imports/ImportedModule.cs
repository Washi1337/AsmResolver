using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Represents a single module that was imported into a portable executable as part of the imports data directory.
    /// Each instance represents one entry in the imports directory.
    /// </summary>
    public class ImportedModule
    {
        private IList<ImportedSymbol>? _members;

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

        /// <summary>
        /// Gets or sets the name of the module that was imported.
        /// </summary>
        public string? Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time stamp that the module was loaded into memory.
        /// </summary>
        /// <remarks>
        /// This field is always 0 if the PE was read from the disk.
        /// </remarks>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the first member that is a forwarder.
        /// </summary>
        public uint ForwarderChain
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of members from the module that were imported.
        /// </summary>
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
        public static ImportedModule? FromReader(PEReaderContext context, ref BinaryStreamReader reader)
        {
            var entry = new SerializedImportedModule(context, ref reader);
            return entry.IsEmpty
                ? null
                : entry;
        }

        /// <summary>
        /// Obtains the collection of members that were imported.
        /// </summary>
        /// <remarks>
        /// This method is called to initialize the value of <see cref="Symbols" /> property.
        /// </remarks>
        /// <returns>The members list.</returns>
        protected virtual IList<ImportedSymbol> GetSymbols() =>
            new OwnedCollection<ImportedModule, ImportedSymbol>(this);

        /// <inheritdoc />
        public override string ToString() => $"{Name} ({Symbols.Count.ToString()} symbols)";
    }
}
