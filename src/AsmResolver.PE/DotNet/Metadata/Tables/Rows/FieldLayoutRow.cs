using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the field layout metadata table.
    /// </summary>
    public readonly struct FieldLayoutRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single field layout row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the field layout table.</param>
        /// <returns>The row.</returns>
        public static FieldLayoutRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new FieldLayoutRow(
                reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }
        
        /// <summary>
        /// Creates a new row for the field layout metadata table.
        /// </summary>
        /// <param name="offset">The offset of the field relative to the start of the enclosing structure type.</param>
        /// <param name="field">The index into the Field type referencing the field that this layout was assigned to.</param>
        public FieldLayoutRow(uint offset, uint field)
        {
            Offset = offset;
            Field = field;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.FieldLayout;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Offset,
            1 => Field,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the offset of the field relative to the start of the enclosing structure type.
        /// </summary>
        public uint Offset
        {
            get;
        }

        /// <summary>
        /// Gets an index into the Field type referencing the field that this layout was assigned to.
        /// </summary>
        public uint Field
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Offset);
            writer.WriteIndex(Field, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided field layout row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(FieldLayoutRow other)
        {
            return Offset == other.Offset && Field == other.Field;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is FieldLayoutRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Offset * 397) ^ (int) Field;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Offset:X8}, {Field:X8})";
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