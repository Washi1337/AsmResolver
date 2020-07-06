using System;
using System.Collections;
using System.Collections.Generic;

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
        
        /// <summary>
        /// Creates a new row for the assembly operating system metadata table.
        /// </summary>
        /// <param name="platformId">The identifier of the platform the assembly is targeting.</param>
        /// <param name="majorVersion">The major version of the platform the assembly is targeting.</param>
        /// <param name="minorVersion">The minor version of the platform the assembly is targeting.</param>
        public AssemblyOSRow(uint platformId, uint majorVersion, uint minorVersion)
        {
            PlatformId = platformId;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.AssemblyOS;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => PlatformId,
            1 => MajorVersion,
            2 => MinorVersion,
            _ => throw new IndexOutOfRangeException()
        };
        
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
        public override bool Equals(object obj) => obj is AssemblyOSRow other && Equals(other);

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

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator()
        {
            return new MetadataRowColumnEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}