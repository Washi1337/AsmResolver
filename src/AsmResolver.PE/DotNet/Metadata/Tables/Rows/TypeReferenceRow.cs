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
    /// Represents a single row in the type reference metadata table.
    /// </summary>
    public struct TypeReferenceRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single type reference row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the type reference table.</param>
        /// <returns>The row.</returns>
        public static TypeReferenceRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new TypeReferenceRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        public TypeReferenceRow(uint resolutionScope, uint name, uint ns)
        {
            ResolutionScope = resolutionScope;
            Name = name;
            Namespace = ns;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.TypeRef;

        /// <summary>
        /// Gets a ResolutionScope coded index (an index to a row in either the Module, ModuleRef, AssemblyRef or TypeRef table)
        /// containing the scope that can resolve this type reference. 
        /// </summary>
        public uint ResolutionScope
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({ResolutionScope:X8}, {Name:X8}, {Namespace:X8})";
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided module row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(TypeReferenceRow other)
        {
            return ResolutionScope == other.ResolutionScope 
                   && Name == other.Name 
                   && Namespace == other.Namespace;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TypeReferenceRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) ResolutionScope;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Namespace;
                return hashCode;
            }
        }
        
    }
}