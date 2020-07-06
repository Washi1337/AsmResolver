using System;
using System.Collections;
using System.Collections.Generic;

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

        /// <summary>
        /// Creates a new row for the module definition metadata table.
        /// </summary>
        /// <param name="generation">The generation number of the module.</param>
        /// <param name="name">The index into the #Strings heap containing the name of the module. </param>
        /// <param name="mvid">The index into the #GUID heap containing the unique identifier to distinguish
        /// between two versions of the same module.</param>
        /// <param name="encId"></param>
        /// <param name="encBaseId"></param>
        public ModuleDefinitionRow(ushort generation, uint name, uint mvid, uint encId, uint encBaseId)
        {
            Generation = generation;
            Name = name;
            Mvid = mvid;
            EncId = encId;
            EncBaseId = encBaseId;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Module;

        /// <inheritdoc />
        public int Count => 5;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Generation,
            1 => Name,
            2 => Mvid,
            3 => EncId,
            4 => EncBaseId,
            _ => throw new IndexOutOfRangeException()
        };

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

        /// <summary>
        /// Gets an index into the #GUID heap containing the unique identifier to distinguish between two
        /// edit-and-continue generations.
        /// </summary>
        public uint EncId
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #GUID heap containing the base identifier of an edit-and-continue generation.
        /// </summary>
        public uint EncBaseId
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16(Generation);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Mvid, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(EncId, (IndexSize) layout.Columns[3].Size);
            writer.WriteIndex(EncBaseId, (IndexSize) layout.Columns[4].Size);
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