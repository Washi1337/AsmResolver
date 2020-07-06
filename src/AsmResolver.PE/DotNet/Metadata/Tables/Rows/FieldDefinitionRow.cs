using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the field definition metadata table.
    /// </summary>
    public readonly struct FieldDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single field definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the field definition table.</param>
        /// <returns>The row.</returns>
        public static FieldDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new FieldDefinitionRow(
                (FieldAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <summary>
        /// Creates a new row for the field definition metadata table.
        /// </summary>
        /// <param name="attributes">The attributes associated to the field definition.</param>
        /// <param name="name">The index into the #Strings heap containing the name of the type reference.</param>
        /// <param name="signature">The index into the #Blob heap containing the signature of the field.</param>
        public FieldDefinitionRow(FieldAttributes attributes, uint name, uint signature)
        {
            Attributes = attributes;
            Name = name;
            Signature = signature;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Field;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => Name,
            2 => Signature,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the attributes associated to the field definition.
        /// </summary>
        public FieldAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings heap containing the name of the type reference.
        /// </summary>
        /// <remarks>
        /// This value should always index a non-empty string.
        /// </remarks>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob heap containing the signature of the field. This includes the field type.
        /// </summary>
        /// <remarks>
        /// This value should always index a valid field signature.
        /// </remarks>
        public uint Signature
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Signature, (IndexSize) layout.Columns[2].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided field definition row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(FieldDefinitionRow other)
        {
            return Attributes == other.Attributes && Name == other.Name && Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is FieldDefinitionRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Signature;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(ushort) Attributes:X4}, {Name:X8}, {Signature:X8})";
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