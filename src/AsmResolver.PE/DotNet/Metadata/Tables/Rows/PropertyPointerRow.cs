using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the property pointer metadata table.
    /// </summary>
    public readonly struct PropertyPointerRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single property pointer row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the property pointer table.</param>
        /// <returns>The row.</returns>
        public static PropertyPointerRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new PropertyPointerRow(reader.ReadIndex((IndexSize) layout.Columns[0].Size));
        }
        
        /// <summary>
        /// Creates a new row for the property pointer metadata table.
        /// </summary>
        /// <param name="property">The index into the Property table that this pointer references.</param>
        public PropertyPointerRow(uint property)
        {
            Property = property;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.PropertyPtr;

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Property,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets an index into the Property table that this pointer references.
        /// </summary>
        public uint Property
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Property, (IndexSize) layout.Columns[0].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided property pointer row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(PropertyPointerRow other)
        {
            return Property == other.Property;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is PropertyPointerRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) Property;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Property:X8})";
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