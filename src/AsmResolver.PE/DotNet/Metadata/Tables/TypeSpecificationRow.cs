using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the type specification metadata table.
    /// </summary>
    public struct TypeSpecificationRow : IMetadataRow, IEquatable<TypeSpecificationRow>
    {
        /// <summary>
        /// Reads a single type specification row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the type specification table.</param>
        /// <returns>The row.</returns>
        public static TypeSpecificationRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
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
        /// Gets or sets an index into the #Blob stream referencing the type signature that was exposed by this row.
        /// </summary>
        public uint Signature
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Signature, (IndexSize) layout.Columns[0].Size);
        }

        /// <inheritdoc />
        public bool Equals(TypeSpecificationRow other)
        {
            return Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
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

        /// <summary>
        /// Determines whether two rows are considered equal.
        /// </summary>
        public static bool operator ==(TypeSpecificationRow left, TypeSpecificationRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(TypeSpecificationRow left, TypeSpecificationRow right) => !(left == right);
    }
}
