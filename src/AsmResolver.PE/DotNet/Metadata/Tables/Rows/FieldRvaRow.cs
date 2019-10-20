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
    /// Represents a single row in the implementation map metadata table.
    /// </summary>
    public readonly struct FieldRvaRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single field RVA row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the field RVA table.</param>
        /// <returns>The row.</returns>
        public static FieldRvaRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new FieldRvaRow(
                reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }
        
        public FieldRvaRow(uint rva, uint field)
        {
            Rva = rva;
            Field = field;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.FieldRva;

        /// <summary>
        /// Gets the virtual address referencing the initial value of the field. 
        /// </summary>
        public uint Rva
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
        
        /// <summary>
        /// Determines whether this row is considered equal to the provided field RVA row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(FieldRvaRow other)
        {
            return Rva == other.Rva && Field == other.Field;
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
                return ((int) Rva * 397) ^ (int) Field;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Rva:X8}, {Field:X8})";
        }
        
    }
}