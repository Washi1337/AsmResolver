using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the assembly reference operating system metadata table.
    /// </summary>
    public struct AssemblyRefOSRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single assembly reference operating system row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the assembly reference operating system table.</param>
        /// <returns>The row.</returns>
        public static AssemblyRefOSRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new AssemblyRefOSRow(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size));
        }

        /// <summary>
        /// Creates a new row for the assembly reference operating system metadata table.
        /// </summary>
        /// <param name="platformId">The identifier of the platform the assembly is targeting.</param>
        /// <param name="majorVersion">The major version of the platform the assembly is targeting.</param>
        /// <param name="minorVersion">The minor version of the platform the assembly is targeting.</param>
        /// <param name="assemblyReference">The index of the AssemblyRef that this operating system row
        /// was assigned to.</param>
        public AssemblyRefOSRow(uint platformId, uint majorVersion, uint minorVersion, uint assemblyReference)
        {
            PlatformId = platformId;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            AssemblyReference = assemblyReference;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.AssemblyRefOS;

        /// <inheritdoc />
        public int Count => 4;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => PlatformId,
            1 => MajorVersion,
            2 => MinorVersion,
            3 => AssemblyReference,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets the identifier of the platform the assembly is targeting.
        /// </summary>
        public uint PlatformId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version of the platform the assembly is targeting.
        /// </summary>
        public uint MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version of the platform the assembly is targeting.
        /// </summary>
        public uint MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the AssemblyRef table referencing the assembly reference that this operating system row
        /// was assigned to.
        /// </summary>
        public uint AssemblyReference
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(PlatformId);
            writer.WriteUInt32(MajorVersion);
            writer.WriteUInt32(MinorVersion);
            writer.WriteIndex(AssemblyReference, (IndexSize) layout.Columns[3].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided assembly operating system row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(AssemblyRefOSRow other)
        {
            return PlatformId == other.PlatformId
                   && MajorVersion == other.MajorVersion
                   && MinorVersion == other.MinorVersion
                   && AssemblyReference == other.AssemblyReference;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is AssemblyRefOSRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) PlatformId;
                hashCode = (hashCode * 397) ^ (int) MajorVersion;
                hashCode = (hashCode * 397) ^ (int) MinorVersion;
                hashCode = (hashCode * 397) ^ (int) AssemblyReference;
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
