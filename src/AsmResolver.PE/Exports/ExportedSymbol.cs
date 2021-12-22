using System.Diagnostics.CodeAnalysis;
using AsmResolver.Collections;

namespace AsmResolver.PE.Exports
{
    /// <summary>
    /// Represents a single symbol that is exported by a dynamically linked library.
    /// </summary>
    public class ExportedSymbol : IOwnedCollectionElement<IExportDirectory>, ISymbol
    {
        /// <summary>
        /// Creates a new symbol that is exported by ordinal.
        /// </summary>
        /// <param name="address">The reference to the segment representing the symbol.</param>
        public ExportedSymbol(ISegmentReference address)
        {
            Name = null;
            Address = address;
        }

        /// <summary>
        /// Creates a new symbol that is exported by name.
        /// </summary>
        /// <param name="address">The reference to the segment representing the symbol.</param>
        /// <param name="name">The name of the symbol.</param>
        public ExportedSymbol(ISegmentReference address, string? name)
        {
            Name = name;
            Address = address;
        }

        /// <summary>
        /// Gets the export directory this symbol was added to (if available).
        /// </summary>
        public IExportDirectory? ParentDirectory
        {
            get;
            private set;
        }

        IExportDirectory? IOwnedCollectionElement<IExportDirectory>.Owner
        {
            get => ParentDirectory;
            set => ParentDirectory = value;
        }

        internal int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the ordinal of the exported symbol.
        /// </summary>
        public uint Ordinal => Index == -1 ? 0u : (uint) Index + (ParentDirectory?.BaseOrdinal ?? 0);

        /// <summary>
        /// Gets a value indicating whether the symbol is exported by ordinal number.
        /// </summary>
        [MemberNotNullWhen(false, nameof(Name))]
        public bool IsByOrdinal => Name is null;

        /// <summary>
        /// Gets a value indicating whether the symbol is exported by name.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Name))]
        public bool IsByName => Name is not null;

        /// <summary>
        /// Gets or sets the name of the exported symbol.
        /// </summary>
        public string? Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reference to the segment representing the symbol.
        /// </summary>
        /// <remarks>
        /// For exported functions, this reference points to the first instruction that is executed.
        /// For exported fields, this reference points to the first byte of data that this field consists of.
        /// </remarks>
        public ISegmentReference Address
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string displayName = Name ?? $"#{Ordinal.ToString()}";
            return ParentDirectory is null
                ? displayName
                : $"{ParentDirectory.Name}!{displayName}";
        }

        /// <inheritdoc />
        public ISegmentReference GetReference() => Address;
    }
}
