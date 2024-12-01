using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Portable PDB local variable metadata table.
    /// </summary>
    public struct LocalVariableRow : IMetadataRow, IEquatable<LocalVariableRow>
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
        /// Reads a single Portable PDB local variable row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the local variable table.</param>
        /// <returns>The row.</returns>
        public static LocalVariableRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new LocalVariableRow(
                (LocalVariableAttributes) reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteUInt16(Index);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[2].Size);
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Determines whether two rows are considered equal.
        /// </summary>
        public static bool operator ==(LocalVariableRow left, LocalVariableRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(LocalVariableRow left, LocalVariableRow right) => !(left == right);
    }
}
