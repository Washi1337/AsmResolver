using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the implementation map metadata table.
    /// </summary>
    public readonly struct ImplementationMapRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single implementation map row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the implementation map table.</param>
        /// <returns>The row.</returns>
        public static ImplementationMapRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new ImplementationMapRow(
                (ImplementationMapAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size));
        }

        /// <summary>
        /// Creates a new row for the implementation map metadata table.
        /// </summary>
        /// <param name="attributes">The attributes associated to the implementation mapping.</param>
        /// <param name="memberForwarded">The MemberForwarded index (an index into either the Field or Method table)
        /// indicating the member that was assigned P/Invoke information.</param>
        /// <param name="importName">The index into the #Strings stream referencing the name of the imported member.</param>
        /// <param name="importScope">The  index into the ModuleRef table indicating the module that this imported member defines.</param>
        public ImplementationMapRow(ImplementationMapAttributes attributes, uint memberForwarded, uint importName, uint importScope)
        {
            Attributes = attributes;
            MemberForwarded = memberForwarded;
            ImportName = importName;
            ImportScope = importScope;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.ImplMap;

        /// <inheritdoc />
        public int Count => 4;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => MemberForwarded,
            2 => ImportName,
            3 => ImportScope,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the attributes associated to the implementation mapping.
        /// </summary>
        public ImplementationMapAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets a MemberForwarded index (an index into either the Field or Method table) indicating the member that was
        /// assigned P/Invoke information.
        /// </summary>
        public uint MemberForwarded
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings stream referencing the name of the imported member.
        /// </summary>
        public uint ImportName
        {
            get;
        }

        /// <summary>
        /// Gets an index into the ModuleRef table indicating the module that this imported member defines.
        /// </summary>
        public uint ImportScope
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteIndex(MemberForwarded, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(ImportName, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(ImportScope, (IndexSize) layout.Columns[3].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided implementation map row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ImplementationMapRow other)
        {
            return Attributes == other.Attributes
                   && MemberForwarded == other.MemberForwarded 
                   && ImportName == other.ImportName 
                   && ImportScope == other.ImportScope;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ImplementationMapRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) MemberForwarded;
                hashCode = (hashCode * 397) ^ (int) ImportName;
                hashCode = (hashCode * 397) ^ (int) ImportScope;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) Attributes:X4}, {MemberForwarded:X8}, {ImportName:X8}, {ImportScope:X8})";
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