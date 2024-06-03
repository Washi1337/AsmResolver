using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Portable PDB import scope metadata table.
    /// </summary>
    public struct ImportScopeRow : IMetadataRow
    {
        /// <summary>
        /// Creates a new row for the Portable PDB import scope metadata table.
        /// </summary>
        /// <param name="parent">
        /// An index into the import parent scope defining the parent scope, or 0 if it is the root scope.
        /// </param>
        /// <param name="imports">
        /// An index into the blob stream referencing the imports that this scope defines.
        /// </param>
        public ImportScopeRow(uint parent, uint imports)
        {
            Parent = parent;
            Imports = imports;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.ImportScope;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Parent,
            1 => Imports,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets an index into the import parent scope defining the parent scope, or 0 if it is the root scope.
        /// </summary>
        public uint Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the blob stream referencing the imports that this scope defines.
        /// </summary>
        public uint Imports
        {
            get;
            set;
        }
        /// <summary>
        /// Reads a single Portable PDB import scope row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the import scope table.</param>
        /// <returns>The row.</returns>
        public static ImportScopeRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new ImportScopeRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Imports, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided import scope row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ImportScopeRow other)
        {
            return Parent == other.Parent && Imports == other.Imports;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ImportScopeRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Parent * 397) ^ (int) Imports;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({Parent:X8}, {Imports:X8})";

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator() => new MetadataRowColumnEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
