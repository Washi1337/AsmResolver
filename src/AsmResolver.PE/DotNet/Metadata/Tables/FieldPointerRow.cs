using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the field pointer metadata table.
    /// </summary>
    public struct FieldPointerRow : IMetadataRow, IEquatable<FieldPointerRow>
    {
        /// <summary>
        /// Reads a single field pointer row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the field pointer table.</param>
        /// <returns>The row.</returns>
        public static FieldPointerRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
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
        /// Gets or sets an index into the Field table that this pointer references.
        /// </summary>
        public uint Field
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Field, (IndexSize) layout.Columns[0].Size);
        }

        /// <inheritdoc />
        public bool Equals(FieldPointerRow other)
        {
            return Field == other.Field;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
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

        /// <summary>
        /// Determines whether two rows are considered equal.
        /// </summary>
        public static bool operator ==(FieldPointerRow left, FieldPointerRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(FieldPointerRow left, FieldPointerRow right) => !(left == right);
    }
}
