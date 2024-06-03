using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.Collections;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Represents one member of an external module that was imported into a PE image.
    /// </summary>
    public class ImportedSymbol : IOwnedCollectionElement<ImportedModule>, ISymbol
    {
        private ushort _ordinalOrHint;

        /// <summary>
        /// Creates a new import entry that references a member exposed by ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal of the member to import.</param>
        public ImportedSymbol(ushort ordinal)
        {
            Ordinal = ordinal;
        }

        /// <summary>
        /// Creates a new import entry that references a member exposed by name.
        /// </summary>
        /// <param name="hint">The likely index of the export in the export table.</param>
        /// <param name="name">The name of the export.</param>
        public ImportedSymbol(ushort hint, string name)
        {
            Hint = hint;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the module that defines the symbol.
        /// </summary>
        public ImportedModule? DeclaringModule
        {
            get;
            private set;
        }

        /// <inheritdoc />
        ImportedModule? IOwnedCollectionElement<ImportedModule>.Owner
        {
            get => DeclaringModule;
            set => DeclaringModule = value;
        }

        /// <summary>
        /// Gets or sets the ordinal of the member that was imported.
        /// </summary>
        /// <remarks>
        /// This value is meaningless if <see cref="IsImportByName"/> is <c>true</c>.
        /// </remarks>
        public ushort Ordinal
        {
            get => _ordinalOrHint;
            set
            {
                _ordinalOrHint = value;
                Name = null;
            }
        }

        /// <summary>
        /// Gets or sets the likely index of the member referenced by <see cref="Name"/>, into the export table.
        /// </summary>
        public ushort Hint
        {
            get => _ordinalOrHint;
            set => _ordinalOrHint = value;
        }

        /// <summary>
        /// Gets or sets the name of the member that was imported.
        /// </summary>
        public string? Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the entry in the import address table (IAT).
        /// </summary>
        public ISegmentReference? AddressTableEntry
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating the member is imported by its ordinal.
        /// </summary>
        public bool IsImportByOrdinal => Name is null;

        /// <summary>
        /// Gets a value indicating the member is imported by its name.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Name))]
        public bool IsImportByName => Name is not null;

        /// <inheritdoc />
        public override string ToString()
        {
            string prefix = DeclaringModule is null
                ? string.Empty
                : $"{DeclaringModule.Name}!";

            string addressSpecifier = AddressTableEntry is null
                ? "???"
                : AddressTableEntry.Rva.ToString("X8");

            return IsImportByOrdinal
                ? $"{prefix}#{Ordinal.ToString()} ({addressSpecifier})"
                : $"{prefix}{Name} ({addressSpecifier})";
        }

        /// <inheritdoc />
        public ISegmentReference? GetReference() => AddressTableEntry;
    }
}
