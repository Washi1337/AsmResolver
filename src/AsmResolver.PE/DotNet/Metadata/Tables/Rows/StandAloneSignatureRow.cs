using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the stand-alone signature metadata table.
    /// </summary>
    public readonly struct StandAloneSignatureRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single stand-alone signature row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the stand-alone signature table.</param>
        /// <returns>The row.</returns>
        public static StandAloneSignatureRow FromReader(IBinaryStreamReader reader, TableLayout layout)
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
        /// Gets an index into the #Blob stream referencing the signature that was exposed by this row.
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
        /// Determines whether this row is considered equal to the provided stand-alone signature row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(StandAloneSignatureRow other)
        {
            return Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
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
    }
}