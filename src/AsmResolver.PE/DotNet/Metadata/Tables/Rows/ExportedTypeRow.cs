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
    /// Represents a single row in the exported type metadata table.
    /// </summary>
    public readonly struct ExportedTypeRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single exported type row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the exported type table.</param>
        /// <returns>The row.</returns>
        public static ExportedTypeRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new ExportedTypeRow(
                (TypeAttributes) reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size),
                reader.ReadIndex((IndexSize) layout.Columns[4].Size));
        }

        public ExportedTypeRow(TypeAttributes attributes, uint typeDefinitionId, uint name, uint ns, uint implementation)
        {
            Attributes = attributes;
            TypeDefinitionId = typeDefinitionId;
            Name = name;
            Namespace = ns;
            Implementation = implementation;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.ExportedType;

        /// <summary>
        /// Gets the attributes associated to the exported type.
        /// </summary>
        public TypeAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets the RID hint of the type definition that was exported.
        /// </summary>
        /// <remarks>
        /// This field is used as a hint only. If the entry in the table does not match the name and namespace referenced
        /// by <see cref="Name"/> and <see cref="Namespace"/> respectively, then the CLR falls back to a search for the
        /// type definition. 
        /// </remarks>
        public uint TypeDefinitionId
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
        /// Gets an index into the #Strings heap containing the namespace of the type reference.
        /// </summary>
        /// <remarks>
        /// This value can be zero. If it is not, it should always index a non-empty string.
        /// </remarks>
        public uint Namespace
        {
            get;
        }

        /// <summary>
        /// Gets an Implementation index (an index into either the File, ExportedType or AssemblyRef table), indicating
        /// the scope that can be used to resolve the exported type.
        /// </summary>
        public uint Implementation
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32((uint) Attributes);
            writer.WriteUInt32(TypeDefinitionId);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(Namespace, (IndexSize) layout.Columns[3].Size);
            writer.WriteIndex(Implementation, (IndexSize) layout.Columns[4].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided exported type row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ExportedTypeRow other)
        {
            return Attributes == other.Attributes
                   && TypeDefinitionId == other.TypeDefinitionId 
                   && Name == other.Name
                   && Namespace == other.Namespace
                   && Implementation == other.Implementation;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ExportedTypeRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) TypeDefinitionId;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Namespace;
                hashCode = (hashCode * 397) ^ (int) Implementation;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(uint) Attributes:X8}, {TypeDefinitionId:X8}, {Name:X8}, {Namespace:X8}, {Implementation:X8})";
        }
        
    }
}