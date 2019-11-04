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
    /// Represents a single row in the Edit-and-Continue remap metadata table.
    /// </summary>
    public readonly struct EncMapRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single edit-and-continue remap row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the edit-and-continue remap table.</param>
        /// <returns>The row.</returns>
        public static EncMapRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new EncMapRow(reader.ReadUInt32());
        }

        /// <summary>
        /// Creates a new row for the edit-and-continue remap metadata table.
        /// </summary>
        /// <param name="token">The token that was remapped.</param>
        public EncMapRow(MetadataToken token)
        {
            Token = token;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.EncMap;

        /// <summary>
        /// Gets the token that was remapped.
        /// </summary>
        public MetadataToken Token
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided edit-and-continue remap row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(EncMapRow other)
        {
            return Token.Equals(other.Token);
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Token.ToUInt32());
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EncMapRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Token.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Token})";
        }
        
    }
}