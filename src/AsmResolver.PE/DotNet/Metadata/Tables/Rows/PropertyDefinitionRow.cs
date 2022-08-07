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

using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the property definition metadata table.
    /// </summary>
    public struct PropertyDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single property definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the property definition table.</param>
        /// <returns>The row.</returns>
        public static PropertyDefinitionRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new PropertyDefinitionRow(
                (PropertyAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <summary>
        /// Creates a new row for the property definition metadata table.
        /// </summary>
        /// <param name="attributes">The attributes associated to the property definition.</param>
        /// <param name="name">The index into the #Strings stream referencing the name of the property.</param>
        /// <param name="type">The index into the #Blob stream referencing the signature of the property.</param>
        public PropertyDefinitionRow(PropertyAttributes attributes, uint name, uint type)
        {
            Attributes = attributes;
            Name = name;
            Type = type;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Property;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => Name,
            2 => Type,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets the attributes associated to the property definition.
        /// </summary>
        public PropertyAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the #Strings stream referencing the name of the property.
        /// </summary>
        /// <remarks>
        /// This value should always index a non-empty string.
        /// </remarks>
        public uint Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the #Blob stream referencing the signature of the property. This includes the property
        /// type.
        /// </summary>
        public uint Type
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Type, (IndexSize) layout.Columns[2].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided parameter definition row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(PropertyDefinitionRow other)
        {
            return Attributes == other.Attributes && Name == other.Name && Type == other.Type;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is PropertyDefinitionRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) Attributes:X4}, {Name:X8}, {Type:X8})";
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
