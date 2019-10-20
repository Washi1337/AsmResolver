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
    /// Represents a single row in the type specification metadata table.
    /// </summary>
    public readonly struct TypeSpecificationRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single type specification row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the type specification table.</param>
        /// <returns>The row.</returns>
        public static TypeSpecificationRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new TypeSpecificationRow(reader.ReadIndex((IndexSize) layout.Columns[0].Size));
        }

        public TypeSpecificationRow(uint signature)
        {
            Signature = signature;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.ModuleRef;

        /// <summary>
        /// Gets an index into the #Blob stream referencing the type signature that was exposed by this row.
        /// </summary>
        public uint Signature
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided module row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(TypeSpecificationRow other)
        {
            return Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TypeSpecificationRow other && Equals(other);
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
    }
}