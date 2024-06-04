using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AsmResolver.Collections;

namespace AsmResolver.PE.Exports
{
    /// <summary>
    /// Represents a single symbol that is exported by a dynamically linked library.
    /// </summary>
    public class ExportedSymbol : IOwnedCollectionElement<ExportDirectory>, ISymbol
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
        /// Creates a new forwarder symbol that is exported by name.
        /// </summary>
        /// <param name="address">The reference to the segment representing the symbol.</param>
        /// <param name="name">The name of the symbol.</param>
        /// <param name="forwarderName">The name of the forwarded symbol.</param>
        public ExportedSymbol(ISegmentReference address, string? name, string? forwarderName)
        {
            Name = name;
            Address = address;
            ForwarderName = forwarderName;
        }

        /// <summary>
        /// Gets the export directory this symbol was added to (if available).
        /// </summary>
        public ExportDirectory? ParentDirectory
        {
            get;
            private set;
        }

        ExportDirectory? IOwnedCollectionElement<ExportDirectory>.Owner
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
        /// For forwarded symbols, this reference points to the name of the symbol the forwarder is referencing.
        /// </remarks>
        public ISegmentReference Address
        {
            get;
            set;
        }

        /// <summary>
        /// When the symbol is a forwarder symbol, gets or sets the full name of the symbol that this export is forwarded to.
        /// </summary>
        /// <remarks>
        /// For exports by name, this name should be in the format <c>ModuleName.ExportName</c>.
        /// For exports by ordinal, this name should be in the format <c>ModuleName.#1</c>.
        /// Failure in doing so will make the Windows PE loader not able to resolve the forwarder symbol.
        /// </remarks>
        public string? ForwarderName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the symbol is forwarded to another external symbol.
        /// </summary>
        [MemberNotNullWhen(true, nameof(ForwarderName))]
        public bool IsForwarder => ForwarderName is not null;

        /// <summary>
        /// Obtains a name that can be used as a <see cref="ForwarderName"/> in another export.
        /// </summary>
        /// <returns>The name.</returns>
        public string FormatNameAsForwarderSymbol() => FormatFullName('.');

        private string FormatFullName(char separator)
        {
            string displayName = Name ?? $"#{Ordinal.ToString()}";
            if (ParentDirectory is not {Name: { } parentName})
                return displayName;

            if (string.Equals(Path.GetExtension(parentName), ".dll", StringComparison.OrdinalIgnoreCase))
                parentName = Path.GetFileNameWithoutExtension(parentName);

            return $"{parentName}{separator}{displayName}";
        }

        /// <inheritdoc />
        public override string ToString() => FormatFullName('!');

        /// <inheritdoc />
        public ISegmentReference GetReference() => Address;
    }
}
