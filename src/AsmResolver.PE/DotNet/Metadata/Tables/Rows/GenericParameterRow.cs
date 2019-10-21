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
    /// Represents a single row in the generic parameter metadata table.
    /// </summary>
    public readonly struct GenericParameterRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single generic parameter row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the generic parameter table.</param>
        /// <returns>The row.</returns>
        public static GenericParameterRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new GenericParameterRow(
                reader.ReadUInt16(),
                (GenericParameterAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size));
        }
        
        public GenericParameterRow(ushort number, GenericParameterAttributes attributes, uint owner, uint name)
        {
            Number = number;
            Attributes = attributes;
            Owner = owner;
            Name = name;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.GenericParam;

        /// <summary>
        /// Gets the index of the generic parameter.
        /// </summary>
        public ushort Number
        {
            get;
        }

        /// <summary>
        /// Gets the attributes associated to the generic parameter.
        /// </summary>
        public GenericParameterAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets a TypeOrMethodDef index (an index into either the TypeDef or MethodDef table) indicating the owner
        /// of the generic parameter.
        /// </summary>
        public uint Owner
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings stream referencing the name of the generic parameter.
        /// </summary>
        public uint Name
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16(Number);
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteIndex(Owner, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[3].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided generic parameter row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(GenericParameterRow other)
        {
            return Number == other.Number 
                   && Attributes == other.Attributes
                   && Owner == other.Owner 
                   && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is GenericParameterRow other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Number.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Owner;
                hashCode = (hashCode * 397) ^ (int) Name;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"({Number:X4}, {(int) Attributes:X4}, {Owner:X8}, {Name:X8})";
        }
        
    }
}