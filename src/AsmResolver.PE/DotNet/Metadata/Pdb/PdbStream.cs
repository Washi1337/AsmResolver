using System;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Metadata.Pdb
{
    /// <summary>
    /// Represents the metadata stream containing Portable PDB debug data that is associated to a .NET module.
    /// </summary>
    public class PdbStream : SegmentBase, IMetadataStream
    {
        /// <summary>
        /// The default name of a PDB stream, as described in the specification provided by Portable PDB v1.0.
        /// </summary>
        public const string DefaultName = "#Pdb";

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        } = DefaultName;

        /// <inheritdoc />
        public virtual bool CanRead => false;

        /// <summary>
        /// Gets the unique identifier representing the debugging metadata blob content.
        /// </summary>
        public byte[] Id
        {
            get;
        } = new byte[20];

        /// <summary>
        /// Gets or sets the token of the entry point method, or 9 if not applicable.
        /// </summary>
        /// <remarks>
        /// This should be the same value as stored in the metadata header.
        /// </remarks>
        public MetadataToken EntryPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an array of row counts of every portable PDB table in the tables stream.
        /// </summary>
        public uint[] TypeSystemTableRows
        {
            get;
            set;
        }

        /// <summary>
        /// Computes the valid bitmask for the type system table rows referenced by this pdb stream.
        /// </summary>
        /// <returns>The bitmask.</returns>
        public ulong ComputeReferencedTypeSystemTables()
        {
            ulong result = 0;

            for (int i = 0; i < TypeSystemTableRows.Length; i++)
            {
                if (TypeSystemTableRows[i] != 0)
                    result |= 1UL << i;
            }

            return result;
        }

        /// <inheritdoc />
        public virtual BinaryStreamReader CreateReader() => throw new NotSupportedException();

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return 20 // ID
                   + sizeof(uint) // EntryPoint
                   + sizeof(ulong) // ReferencedTypeSystemTables
                   + 4 * (uint) TypeSystemTableRows.Length; // TypeSystemTableRows.
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBytes(Id);
            writer.WriteUInt32(EntryPoint.ToUInt32());
            writer.WriteUInt64(ComputeReferencedTypeSystemTables());
            foreach (uint row in TypeSystemTableRows)
                writer.WriteUInt32(row);
        }

    }
}
