using System;
using System.Diagnostics;
using AsmResolver.Collections;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides a lazy-initialized list of module import entries that is stored in a PE file.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class SerializedImportedModuleList : LazyList<ImportedModule>
    {
        private readonly PEReaderContext _context;
        private readonly DataDirectory _dataDirectory;

        /// <summary>
        /// Prepares a new lazy-initialized list of module import entries.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="dataDirectory">The import data directory.</param>
        public SerializedImportedModuleList(PEReaderContext context, in DataDirectory dataDirectory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dataDirectory = dataDirectory;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            if (!_context.File.TryCreateDataDirectoryReader(_dataDirectory, out var reader))
            {
                _context.BadImage("Invalid import data directory RVA and/or size.");
                return;
            }

            while (true)
            {
                var entry = ImportedModule.FromReader(_context, ref reader);
                if (entry == null)
                    break;
                Items.Add(entry);
            }
        }

    }
}
