using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the field pointer metadata table.
    /// </summary>
    public readonly struct FieldPointerRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single field pointer row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the field pointer table.</param>
        /// <returns>The row.</returns>
        public static FieldPointerRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new FieldPointerRow(reader.ReadIndex((IndexSize) layout.Columns[0].Size));
        }
        
        /// <summary>
        /// Creates a new row for the field pointer metadata table.
        /// </summary>
        /// <param name="field">The index into the Field table that this pointer references.</param>
        public FieldPointerRow(uint field)
        {
            Field = field;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.FieldPtr;

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Field,
            _ => throw new IndexOutOfRangeException()
        };
        
        /// <summary>
        /// Gets an index into the Field table that this pointer references.
        /// </summary>
        public uint Field
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Field, (IndexSize) layout.Columns[0].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided field pointer row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(FieldPointerRow other)
        {
            return Field == other.Field;
        }


        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is FieldPointerRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) Field;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Field:X8})";
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