using System;
using System.Diagnostics;
using AsmResolver.Collections;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides a lazy-initialized list of module import entries that is stored in a PE file.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class SerializedImportedModuleList : LazyList<IImportedModule>
    {
        private readonly IPEFile _peFile;
        private readonly IErrorListener _errorListener;
        private readonly DataDirectory _dataDirectory;

        /// <summary>
        /// Prepares a new lazy-initialized list of module import entries.
        /// </summary>
        /// <param name="peFile">The PE file containing the list of modules.</param>
        /// <param name="errorListener">The object responsible for recording errors.</param>
        /// <param name="dataDirectory">The import data directory.</param>
        public SerializedImportedModuleList(IPEFile peFile, IErrorListener errorListener, DataDirectory dataDirectory)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            _errorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
            _dataDirectory = dataDirectory;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            if (!_peFile.TryCreateDataDirectoryReader(_dataDirectory, out var reader))
            {
                _errorListener.BadImage("Invalid import data directory RVA and/or size.");
                return;
            }

            while (true)
            {
                var entry = ImportedModule.FromReader(_peFile, reader, _errorListener);
                if (entry == null)
                    break;
                Items.Add(entry);
            }
        }
        
    }
}