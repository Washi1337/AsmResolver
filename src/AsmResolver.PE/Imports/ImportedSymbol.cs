// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Represents one member of an external module that was imported into a PE image.
    /// </summary>
    public class ImportedSymbol
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
        public ulong Address
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
            return IsImportByOrdinal
                ? $"#{Ordinal} ({Address:X8})"
                : $"{Name} ({Address:X8})";
        }
        
    }
}