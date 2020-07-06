using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the implementation map metadata table.
    /// </summary>
    public readonly struct FieldRvaRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single field RVA row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the field RVA table.</param>
        /// <param name="referenceResolver"></param>
        /// <returns>The row.</returns>
        public static FieldRvaRow FromReader(IBinaryStreamReader reader, TableLayout layout, ISegmentReferenceResolver referenceResolver)
        {
            return new FieldRvaRow(
                 referenceResolver.GetReferenceToRva(reader.ReadUInt32()),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }
        
        /// <summary>
        /// Creates a new row for the field RVA metadata table.
        /// </summary>
        /// <param name="data">The reference to the start of the initial field data.</param>
        /// <param name="field">The index into the Field table indicating the field that was assigned an initial value.</param>
        public FieldRvaRow(ISegmentReference data, uint field)
        {
            Data = data;
            Field = field;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.FieldRva;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Data?.Rva ?? 0,
            1 => Field,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets a reference to the start of the initial field data. 
        /// </summary>
        /// <remarks>
        /// This field deviates from the original specification as described in ECMA-335. It replaces the RVA column of
        /// the field RVA row. Only the RVA of this reference is only considered when comparing two field RVA rows
        /// for equality.
        /// </remarks>
        public ISegmentReference Data
        {
            get;
        }

        /// <summary>
        /// Gets an index into the Field table indicating the field that was assigned an initial value.
        /// </summary>
        public uint Field
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Data.Rva);
            writer.WriteIndex(Field, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided field RVA row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// When comparing both data fields, only the RVA is considered in this equality test. The exact type is ignored.
        /// </remarks>
        public bool Equals(FieldRvaRow other)
        {
            return Data?.Rva == other.Data?.Rva && Field == other.Field;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is FieldRvaRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Data.Rva * 397) ^ (int) Field;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Data.Rva:X8}, {Field:X8})";
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