using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the stand-alone signature metadata table.
    /// </summary>
    public struct StandAloneSignatureRow : IMetadataRow, IEquatable<StandAloneSignatureRow>
    {
        /// <summary>
        /// Reads a single stand-alone signature row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the stand-alone signature table.</param>
        /// <returns>The row.</returns>
        public static StandAloneSignatureRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new StandAloneSignatureRow(reader.ReadIndex((IndexSize) layout.Columns[0].Size));
        }

        /// <summary>
        /// Creates a new row for the stand-alone signature metadata table.
        /// </summary>
        /// <param name="signature">The index into the #Blob stream referencing the signature that was exposed by this row.</param>
        public StandAloneSignatureRow(uint signature)
        {
            Signature = signature;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.StandAloneSig;

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Signature,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets an index into the #Blob stream referencing the signature that was exposed by this row.
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
        public bool Equals(StandAloneSignatureRow other)
        {
            return Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is StandAloneSignatureRow other && Equals(other);
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
        public static bool operator ==(StandAloneSignatureRow left, StandAloneSignatureRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(StandAloneSignatureRow left, StandAloneSignatureRow right) => !(left == right);
    }
}
