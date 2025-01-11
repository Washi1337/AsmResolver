using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Portable PDB custom debug information metadata table.
    /// </summary>
    public struct CustomDebugInformationRow : IMetadataRow, IEquatable<CustomDebugInformationRow>
    {
        /// <summary>
        /// Creates a new row for the Portable PDB custom debug information metadata table.
        /// </summary>
        /// <param name="parent">
        /// A coded index defining the member that this debug information is associated to.
        /// </param>
        /// <param name="kind">
        /// An index into the GUID stream referencing the type of debug data that is stored in this record.
        /// </param>
        /// <param name="value">
        /// An index into the blob stream referencing the data of the record.
        /// </param>
        public CustomDebugInformationRow(uint parent, uint kind, uint value)
        {
            Parent = parent;
            Kind = kind;
            Value = value;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.CustomDebugInformation;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Parent,
            1 => Kind,
            2 => Value,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets a coded index defining the member that this debug information is associated to.
        /// </summary>
        public uint Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the GUID stream referencing the type of debug data that is stored in this record.
        /// </summary>
        public uint Kind
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the blob stream referencing the data of the record.
        /// </summary>
        public uint Value
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single Portable PDB custom debug information row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the custom debug information table.</param>
        /// <returns>The row.</returns>
        public static CustomDebugInformationRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new CustomDebugInformationRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Kind, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Value, (IndexSize) layout.Columns[2].Size);
        }

        /// <inheritdoc />
        public bool Equals(CustomDebugInformationRow other)
        {
            return Parent == other.Parent && Kind == other.Kind && Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is CustomDebugInformationRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Parent;
                hashCode = (hashCode * 397) ^ (int) Kind;
                hashCode = (hashCode * 397) ^ (int) Value;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({Parent:X8}, {Kind:X8}, {Value:X8})";

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator() => new MetadataRowColumnEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Determines whether two rows are considered equal.
        /// </summary>
        public static bool operator ==(CustomDebugInformationRow left, CustomDebugInformationRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(CustomDebugInformationRow left, CustomDebugInformationRow right) => !(left == right);
    }
}
