using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the parameter definition metadata table.
    /// </summary>
    public readonly struct ParameterDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single parameter definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the parameter definition table.</param>
        /// <returns>The row.</returns>
        public static ParameterDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new ParameterDefinitionRow(
                (ParameterAttributes) reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <summary>
        /// Creates a new row for the parameter definition metadata table. 
        /// </summary>
        /// <param name="attributes">The attributes associated to the parameter.</param>
        /// <param name="sequence">The index of the parameter definition.</param>
        /// <param name="name">The index into the #Strings heap containing the name of the type reference.</param>
        public ParameterDefinitionRow(ParameterAttributes attributes, ushort sequence, uint name)
        {
            Attributes = attributes;
            Sequence = sequence;
            Name = name;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Param;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => Sequence,
            2 => Name,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the attributes associated to the parameter.
        /// </summary>
        public ParameterAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets the index of the parameter definition.
        /// </summary>
        public ushort Sequence
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings heap containing the name of the type reference.
        /// </summary>
        /// <remarks>
        /// If this value is zero, the parameter name is considered <c>null</c>.
        /// </remarks>
        public uint Name
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteUInt16(Sequence);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[2].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided parameter definition row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ParameterDefinitionRow other)
        {
            return Attributes == other.Attributes && Sequence == other.Sequence && Name == other.Name;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ParameterDefinitionRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ Sequence.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Name;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) Attributes:X4}, {Sequence:X4}, {Name:X8})";
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