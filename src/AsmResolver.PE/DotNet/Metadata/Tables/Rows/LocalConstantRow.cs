using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the Portable PDB local constant metadata table.
    /// </summary>
    public struct LocalConstantRow : IMetadataRow
    {
        /// <summary>
        /// Creates a new row for the Portable PDB Local Constant metadata table.
        /// </summary>
        /// <param name="name">An index into the strings stream referencing the name of the constant.</param>
        /// <param name="signature">An index into the blob stream referencing the signature of the constant.</param>
        public LocalConstantRow(uint name, uint signature)
        {
            Name = name;
            Signature = signature;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.LocalConstant;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Name,
            1 => Signature,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets an index into the strings stream referencing the name of the constant.
        /// </summary>
        public uint Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the blob stream referencing the signature of the constant.
        /// </summary>
        public uint Signature
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single Portable PDB local constant row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the local constant table.</param>
        /// <returns>The row.</returns>
        public static LocalConstantRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new LocalConstantRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Name, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Signature, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided local constant row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(LocalConstantRow other)
        {
            return Name == other.Name && Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is LocalConstantRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Name * 397) ^ (int) Signature;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({Name:X8}, {Signature:X8})";

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator() => new MetadataRowColumnEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
