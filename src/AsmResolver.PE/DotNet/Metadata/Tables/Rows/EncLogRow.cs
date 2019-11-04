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
    /// Represents a single row in the Edit-and-Continue log metadata table.
    /// </summary>
    public readonly struct EncLogRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single edit-and-continue log row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the edit-and-continue log table.</param>
        /// <returns>The row.</returns>
        public static EncLogRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new EncLogRow(
                reader.ReadUInt32(),
                (DeltaFunctionCode) reader.ReadUInt32());
        }
        
        /// <summary>
        /// Creates a new row for the edit-and-continue log metadata table.
        /// </summary>
        /// <param name="token">The metadata token to apply the delta function to.</param>
        /// <param name="funcCode">The delta function to apply.</param>
        public EncLogRow(MetadataToken token, DeltaFunctionCode funcCode)
        {
            Token = token;
            FuncCode = funcCode;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.EncLog;

        /// <summary>
        /// Gets the metadata token to apply the delta function to.
        /// </summary>
        public MetadataToken Token
        {
            get;
        }

        /// <summary>
        /// Gets the delta function to apply.
        /// </summary>
        public DeltaFunctionCode FuncCode
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided edit-and-continue log row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(EncLogRow other)
        {
            return Token.Equals(other.Token) && FuncCode == other.FuncCode;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Token.ToUInt32());
            writer.WriteUInt32((uint) FuncCode);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EncLogRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Token.GetHashCode() * 397) ^ (int) FuncCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Token}, {(int) FuncCode:X8})";
        }
        
    }
}