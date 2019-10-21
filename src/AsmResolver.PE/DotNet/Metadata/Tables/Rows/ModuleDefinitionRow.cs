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
    /// Represents a single row in the module definition metadata table.
    /// </summary>
    public readonly struct ModuleDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single module definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the module definition table.</param>
        /// <returns>The row.</returns>
        public static ModuleDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new ModuleDefinitionRow(
                reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size),
                reader.ReadIndex((IndexSize) layout.Columns[4].Size));
        }

        public ModuleDefinitionRow(ushort generation, uint name, uint mvid, uint encId, uint encBaseId)
        {
            Generation = generation;
            Name = name;
            Mvid = mvid;
            EncId = encId;
            EncBaseId = encBaseId;
        }

        public TableIndex TableIndex => TableIndex.Module;

        /// <summary>
        /// Gets the generation number of the module.  
        /// </summary>
        /// <remarks>
        /// This value is reserved and should be set to zero.
        /// </remarks>
        public ushort Generation
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings heap containing the name of the module. 
        /// </summary>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #GUID heap containing the unique identifier to distinguish between two versions
        /// of the same module.
        /// </summary>
        public uint Mvid
        {
            get;
        }

        public uint EncId
        {
            get;
        }

        public uint EncBaseId
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16(Generation);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Mvid, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(EncId, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(EncBaseId, (IndexSize) layout.Columns[1].Size);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Generation:X4}, {Name:X8}, {Mvid:X8}, {EncId:X8}, {EncBaseId:X8})";
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided module row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ModuleDefinitionRow other)
        {
            return Generation == other.Generation
                   && Name == other.Name
                   && Mvid == other.Mvid
                   && EncId == other.EncId
                   && EncBaseId == other.EncBaseId;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ModuleDefinitionRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Generation.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Mvid;
                hashCode = (hashCode * 397) ^ (int) EncId;
                hashCode = (hashCode * 397) ^ (int) EncBaseId;
                return hashCode;
            }
        }
    }
}