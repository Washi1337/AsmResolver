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
    /// Represents a single row in the method specification metadata table.
    /// </summary>
    public readonly struct MethodSpecificationRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single method specification row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the method specification table.</param>
        /// <returns>The row.</returns>
        public static MethodSpecificationRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new MethodSpecificationRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <summary>
        /// Creates a new row for the method specification metadata table.
        /// </summary>
        /// <param name="method">The index into the method definition table indicating the method to be instantiated.</param>
        /// <param name="instantiation">The index into the #Blob stream referencing the instantiation parameters of the method.</param>
        public MethodSpecificationRow(uint method, uint instantiation)
        {
            Method = method;
            Instantiation = instantiation;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.MethodSpec;

        /// <summary>
        /// Gets an index into the method definition table indicating the method to be instantiated.
        /// </summary>
        public uint Method
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob stream referencing the instantiation parameters of the method.
        /// </summary>
        public uint Instantiation
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Method, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Instantiation,(IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided method specification row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(MethodSpecificationRow other)
        {
            return Method == other.Method && Instantiation == other.Instantiation;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is MethodSpecificationRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Method * 397) ^ (int) Instantiation;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Method:X8}, {Instantiation:X8})";
        }
        
    }
}