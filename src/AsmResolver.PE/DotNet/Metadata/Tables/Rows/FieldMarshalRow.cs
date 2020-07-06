using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the field marshal metadata table.
    /// </summary>
    public readonly struct FieldMarshalRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single field marshal row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the field marshal table.</param>
        /// <returns>The row.</returns>
        public static FieldMarshalRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new FieldMarshalRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <summary>
        /// Creates a new row for the field marshal metadata table.
        /// </summary>
        /// <param name="parent">The HasFieldMarshal index (an index into either the Field or Parameter table) that this
        /// field marshaller is assigned to.</param>
        /// <param name="nativeType">The index into the #Blob stream containing the marshaller data.</param>
        public FieldMarshalRow(uint parent, uint nativeType)
        {
            Parent = parent;
            NativeType = nativeType;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.FieldMarshal;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Parent,
            1 => NativeType,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets a HasFieldMarshal index (an index into either the Field or Parameter table) that this field marshaller
        /// is assigned to.
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob stream containing the marshaller data.
        /// </summary>
        public uint NativeType
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(NativeType, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided field marshal row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(FieldMarshalRow other)
        {
            return Parent == other.Parent && NativeType == other.NativeType;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is FieldMarshalRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Parent * 397) ^ (int) NativeType;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Parent:X8}, {NativeType:X8})";
        }

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator()
        {
            return new MetadataRowColumnEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}