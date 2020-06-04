using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Lazy;

namespace AsmResolver.PE.Exports
{
    /// <summary>
    /// Represents the data directory containing exported symbols that other images can access through dynamic linking.
    /// </summary>
    public class ExportDirectory
    {
        private readonly LazyVariable<string> _name;
        private IList<ExportedSymbol> _exports;

        /// <summary>
        /// Initializes a new empty symbol export directory.
        /// </summary>
        protected ExportDirectory()
        {
            _name = new LazyVariable<string>(GetName);
        }

        /// <summary>
        /// Creates a new symbol export directory.
        /// </summary>
        /// <param name="name">The name of the library exporting the symbols.</param>
        public ExportDirectory(string name)
            : this()
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets or sets the flags associated to the export directory.
        /// </summary>
        /// <remarks>
        /// This field is reserved and should be zero.
        /// </remarks>
        public uint ExportFlags
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// Gets or sets the time and date that the exports data was created.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user major version number.
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user minor version number.
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the exports directory.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets an ordered list of symbols that are exported by the portable executable (PE) image.
        /// </summary>
        public IList<ExportedSymbol> Exports
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
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the list of exported symbols defined by the export directory.
        /// </summary>
        /// <returns>The exported symbols..</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Exports"/> property.
        /// </remarks>
        protected virtual IList<ExportedSymbol> GetExports() => new List<ExportedSymbol>();
    }
}