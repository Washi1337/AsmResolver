using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the manifest resource metadata table.
    /// </summary>
    public readonly struct ManifestResourceRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single manifest resource row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the manifest resource table.</param>
        /// <returns>The row.</returns>
        public static ManifestResourceRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new ManifestResourceRow(
                reader.ReadUInt32(),
                (ManifestResourceAttributes) reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size));
        }
        
        /// <summary>
        /// Creates a new row for the manifest resource metadata table.
        /// </summary>
        /// <param name="offset">The byte offset within the referenced file at which the resource record begins. </param>
        /// <param name="attributes">The attributes associated with this resource.</param>
        /// <param name="name">The index into the #Strings heap referencing the name of the resource.</param>
        /// <param name="implementation">The Implementation index (an index into either the File, AssemblyRef or
        /// ExportedType table) indicating the file that contains the resource data. </param>
        public ManifestResourceRow(uint offset, ManifestResourceAttributes attributes, uint name, uint implementation)
        {
            Offset = offset;
            Attributes = attributes;
            Name = name;
            Implementation = implementation;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.ManifestResource;

        /// <inheritdoc />
        public int Count => 4;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Offset,
            1 => (uint) Attributes,
            2 => Name,
            3 => Implementation,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the byte offset within the referenced file at which the resource record begins. 
        /// </summary>
        public uint Offset
        {
            get;
        }

        /// <summary>
        /// Gets the attributes associated with this resource.
        /// </summary>
        public ManifestResourceAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings heap referencing the name of the resource.
        /// </summary>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Gets an Implementation index (an index into either the File, AssemblyRef or ExportedType table) indicating
        /// the file that contains the resource data. 
        /// </summary>
        /// <remarks>
        /// When this field is set to zero, the resource data is embedded into the current assembly.
        /// </remarks>
        public uint Implementation
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Offset);
            writer.WriteUInt32((uint) Attributes);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(Implementation, (IndexSize) layout.Columns[3].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided manifest resource row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ManifestResourceRow other)
        {
            return Offset == other.Offset 
                   && Attributes == other.Attributes
                   && Name == other.Name 
                   && Implementation == other.Implementation;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ManifestResourceRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Offset;
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Implementation;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Offset:X8}, {(int) Attributes:X8}, {Name:X8}, {Implementation:X8})";
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