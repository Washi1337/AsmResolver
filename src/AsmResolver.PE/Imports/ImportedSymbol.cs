using System;
using AsmResolver.Collections;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Represents one member of an external module that was imported into a PE image.
    /// </summary>
    public class ImportedSymbol : IOwnedCollectionElement<IImportedModule>, ISymbol
    {
        private ushort _ordinalOrHint;
        private string _name;

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
        public IImportedModule DeclaringModule
        {
            get;
            private set;
        }

        /// <inheritdoc />
        IImportedModule IOwnedCollectionElement<IImportedModule>.Owner
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
                _name = null;
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
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// Gets or sets the entry in the import address table (IAT). 
        /// </summary>
        public ISegmentReference AddressTableEntry
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating the member is imported by its ordinal.
        /// </summary>
        public bool IsImportByOrdinal => Name == null;

        /// <summary>
        /// Gets a value indicating the member is imported by its name.
        /// </summary>
        public bool IsImportByName => Name != null;

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
        public ISegmentReference GetReference() => AddressTableEntry;
    }
}