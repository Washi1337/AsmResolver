using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;

namespace AsmResolver.PE.Exports
{
    /// <summary>
    /// Provides a basic implementation of the <see cref="IExportDirectory"/> interface.
    /// </summary>
    public class ExportDirectory : IExportDirectory
    {
        private readonly LazyVariable<ExportDirectory, string?> _name;
        private IList<ExportedSymbol>? _exports;

        /// <summary>
        /// Initializes a new empty symbol export directory.
        /// </summary>
        protected ExportDirectory()
        {
            _name = new LazyVariable<ExportDirectory, string?>(x => x.GetName());
        }

        /// <summary>
        /// Creates a new symbol export directory.
        /// </summary>
        /// <param name="name">The name of the library exporting the symbols.</param>
        public ExportDirectory(string name)
        {
            _name = new LazyVariable<ExportDirectory, string?>(name ?? throw new ArgumentNullException(nameof(name)));
        }

        /// <inheritdoc />
        public uint ExportFlags
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint TimeDateStamp
        {
            get;
            set;
        } = 0xFFFFFFFF;

        /// <inheritdoc />
        public ushort MajorVersion
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string? Name
        {
            get => _name.GetValue(this);
            set => _name.SetValue(value);
        }

        /// <inheritdoc />
        public uint BaseOrdinal
        {
            get;
            set;
        } = 1;

        /// <inheritdoc />
        public IList<ExportedSymbol> Entries
        {
            get
            {
                if (_exports is null)
                    Interlocked.CompareExchange(ref _exports, GetExports(), null);
                return _exports;
            }
        }

        /// <summary>
        /// Obtains the name of the library that is exporting symbols.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string? GetName() => null;

        /// <summary>
        /// Obtains the list of exported symbols defined by the export directory.
        /// </summary>
        /// <returns>The exported symbols..</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Entries"/> property.
        /// </remarks>
        protected virtual IList<ExportedSymbol> GetExports() =>
            new ExportedSymbolCollection(this);
    }
}
