using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the type specification metadata table.
    /// </summary>
    public readonly struct TypeSpecificationRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single type specification row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the type specification table.</param>
        /// <returns>The row.</returns>
        public static TypeSpecificationRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new TypeSpecificationRow(reader.ReadIndex((IndexSize) layout.Columns[0].Size));
        }

        /// <summary>
        /// Creates a new row for the type specification metadata table.
        /// </summary>
        /// <param name="signature">The index into the #Blob stream referencing the type signature that was exposed by
        /// this row.</param>
        public TypeSpecificationRow(uint signature)
        {
            Signature = signature;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.ModuleRef;

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Signature,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets an index into the #Blob stream referencing the type signature that was exposed by this row.
        /// </summary>
        public uint Signature
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Signature, (IndexSize) layout.Columns[0].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided module row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(TypeSpecificationRow other)
        {
            return Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TypeSpecificationRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) Signature;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Signature:X8})";
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