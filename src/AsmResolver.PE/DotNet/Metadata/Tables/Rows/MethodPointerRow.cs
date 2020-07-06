using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the method pointer metadata table.
    /// </summary>
    public readonly struct MethodPointerRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single field pointer row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the field pointer table.</param>
        /// <returns>The row.</returns>
        public static MethodPointerRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new MethodPointerRow(reader.ReadIndex((IndexSize) layout.Columns[0].Size));
        }
        
        /// <summary>
        /// Creates a new row for the method pointer metadata table.
        /// </summary>
        /// <param name="method">The index into the Method table that this pointer references.</param>
        public MethodPointerRow(uint method)
        {
            Method = method;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.MethodPtr;

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Method,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets an index into the Method table that this pointer references.
        /// </summary>
        public uint Method
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Method, (IndexSize) layout.Columns[0].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided method pointer row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(MethodPointerRow other)
        {
            return Method == other.Method;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is MethodPointerRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) Method;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Method:X8})";
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