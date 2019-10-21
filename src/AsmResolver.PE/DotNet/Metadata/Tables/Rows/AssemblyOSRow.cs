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
    /// Represents a single row in the assembly operating system metadata table.
    /// </summary>
    public readonly struct AssemblyOSRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single assembly operating system row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the assembly operating system table.</param>
        /// <returns>The row.</returns>
        public static AssemblyOSRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new AssemblyOSRow(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32());
        }
        
        public AssemblyOSRow(uint platformId, uint majorVersion, uint minorVersion)
        {
            PlatformId = platformId;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.AssemblyOS;

        /// <summary>
        /// Gets the identifier of the platform the assembly is targeting.
        /// </summary>
        public uint PlatformId
        {
            get;
        }

        /// <summary>
        /// Gets the major version of the platform the assembly is targeting.
        /// </summary>
        public uint MajorVersion
        {
            get;
        }

        /// <summary>
        /// Gets the minor version of the platform the assembly is targeting.
        /// </summary>
        public uint MinorVersion
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(PlatformId);
            writer.WriteUInt32(MajorVersion);
            writer.WriteUInt32(MinorVersion);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided assembly operating system row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(AssemblyOSRow other)
        {
            return PlatformId == other.PlatformId 
                   && MajorVersion == other.MajorVersion
                   && MinorVersion == other.MinorVersion;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is AssemblyOSRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) PlatformId;
                hashCode = (hashCode * 397) ^ (int) MajorVersion;
                hashCode = (hashCode * 397) ^ (int) MinorVersion;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({PlatformId:X8}, {MajorVersion:X8}, {MinorVersion:X8})";
        }
    }
}