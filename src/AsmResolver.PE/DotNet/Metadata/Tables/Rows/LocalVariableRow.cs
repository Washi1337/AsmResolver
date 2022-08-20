using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the Portable PDB local variable metadata table.
    /// </summary>
    public struct LocalVariableRow : IMetadataRow
    {
        /// <summary>
        /// Creates a new row for the Portable PDB Local Variable metadata table.
        /// </summary>
        /// <param name="attributes">The attributes associated to the local variable.</param>
        /// <param name="index">The index of the local variable.</param>
        /// <param name="name">An index into the strings stream referencing the name of the local variable.</param>
        public LocalVariableRow(LocalVariableAttributes attributes, ushort index, uint name)
        {
            Attributes = attributes;
            Index = index;
            Name = name;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.LocalVariable;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => Index,
            2 => Name,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets the attributes associated to the local variable.
        /// </summary>
        public LocalVariableAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the local variable.
        /// </summary>
        public ushort Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the strings stream referencing the name of the local variable.
        /// </summary>
        public uint Name
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single Portable PDB Method Debug Information row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the assembly definition table.</param>
        /// <returns>The row.</returns>
        public static LocalVariableRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new LocalVariableRow(
                (LocalVariableAttributes) reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteUInt16(Index);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[2].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided local variable row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(LocalVariableRow other)
        {
            return Attributes == other.Attributes
                   && Index == other.Index
                   && Name == other.Name;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is LocalScopeRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ Index;
                hashCode = (hashCode * 397) ^ (int) Name;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({(ushort) Attributes:X4}, {Index:X4}, {Name:X8})";

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator() => new MetadataRowColumnEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
