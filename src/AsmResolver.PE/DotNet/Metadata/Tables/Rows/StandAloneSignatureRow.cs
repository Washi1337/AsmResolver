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
    /// Represents a single row in the stand-alone signature metadata table.
    /// </summary>
    public readonly struct StandAloneSignatureRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single stand-alone signature row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the stand-alone signature table.</param>
        /// <returns>The row.</returns>
        public static StandAloneSignatureRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new StandAloneSignatureRow(reader.ReadIndex((IndexSize) layout.Columns[0].Size));
        }
        
        public StandAloneSignatureRow(uint signature)
        {
            Signature = signature;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.StandAloneSig;

        /// <summary>
        /// Gets an index into the #Blob stream referencing the signature that was exposed by this row.
        /// </summary>
        public uint Signature
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Signature, (IndexSize) layout.Columns[0].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided stand-alone signature row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(StandAloneSignatureRow other)
        {
            return Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is StandAloneSignatureRow other && Equals(other);
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