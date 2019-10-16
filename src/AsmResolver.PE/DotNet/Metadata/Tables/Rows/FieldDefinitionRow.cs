// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

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
        /// <param name="layout">The layout of the field pointer table.</param>
        /// <returns>The row.</returns>
        public static FieldDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new FieldDefinitionRow(
                (FieldAttributes) reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        public FieldDefinitionRow(FieldAttributes attributes, uint name, uint signature)
        {
            Attributes = attributes;
            Name = name;
            Signature = signature;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Field;

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
        public uint Signature
        {
            get;
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
        
    }
}