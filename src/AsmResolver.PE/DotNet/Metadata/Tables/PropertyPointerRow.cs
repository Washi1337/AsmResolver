using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the property pointer metadata table.
    /// </summary>
    public struct PropertyPointerRow : IMetadataRow, IEquatable<PropertyPointerRow>
    {
        /// <summary>
        /// Reads a single property pointer row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the property pointer table.</param>
        /// <returns>The row.</returns>
        public static PropertyPointerRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
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
        /// Gets or sets an index into the Property table that this pointer references.
        /// </summary>
        public uint Property
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Property, (IndexSize) layout.Columns[0].Size);
        }

        /// <inheritdoc />
        public bool Equals(PropertyPointerRow other)
        {
            return Property == other.Property;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
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

        /// <summary>
        /// Determines whether two rows are considered equal.
        /// </summary>
        public static bool operator ==(PropertyPointerRow left, PropertyPointerRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(PropertyPointerRow left, PropertyPointerRow right) => !(left == right);
    }
}
