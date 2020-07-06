using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the nested class metadata table.
    /// </summary>
    public readonly struct NestedClassRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single nested class row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the nested class table.</param>
        /// <returns>The row.</returns>
        public static NestedClassRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new NestedClassRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }
        
        /// <summary>
        /// Creates a new row for the nested class metadata table.
        /// </summary>
        /// <param name="nestedClass">The index into the TypeDef table indicating the class that was nested into another class.</param>
        /// <param name="enclosingClass">The index into the TypeDef table indicating the enclosing class.</param>
        public NestedClassRow(uint nestedClass, uint enclosingClass)
        {
            NestedClass = nestedClass;
            EnclosingClass = enclosingClass;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.NestedClass;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => NestedClass,
            1 => EnclosingClass,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets an index into the TypeDef table indicating the class that was nested into another class.
        /// </summary>
        public uint NestedClass
        {
            get;
        }

        /// <summary>
        /// Gets an index into the TypeDef table indicating the enclosing class.
        /// </summary>
        public uint EnclosingClass
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(NestedClass, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(EnclosingClass, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided nested class row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(NestedClassRow other)
        {
            return NestedClass == other.NestedClass && EnclosingClass == other.EnclosingClass;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is NestedClassRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) NestedClass * 397) ^ (int) EnclosingClass;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({NestedClass:X8}, {EnclosingClass:X8})";
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