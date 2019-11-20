using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the assembly reference processor metadata table.
    /// </summary>
    public readonly struct AssemblyRefProcessorRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single assembly reference processor row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the assembly reference processor table.</param>
        /// <returns>The row.</returns>
        public static AssemblyRefProcessorRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new AssemblyRefProcessorRow(
                reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }
        
        /// <summary>
        /// Creates a new row for the assembly processor metadata table.
        /// </summary>
        /// <param name="processorId">The processor identifier the assembly is targeting.</param>
        /// <param name="assemblyReference">The index of the AssemblyRef that this processor row was assigned to.</param>
        public AssemblyRefProcessorRow(uint processorId, uint assemblyReference)
        {
            ProcessorId = processorId;
            AssemblyReference = assemblyReference;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.AssemblyRefProcessor;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => ProcessorId,
            1 => AssemblyReference,
            _ => throw new IndexOutOfRangeException()
        };
        
        /// <summary>
        /// Gets the processor identifier the assembly is targeting.
        /// </summary>
        public uint ProcessorId
        {
            get;
        }

        /// <summary>
        /// Gets an index into the AssemblyRef table referencing the assembly reference that this processor row
        /// was assigned to.
        /// </summary>
        public uint AssemblyReference
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(ProcessorId);
            writer.WriteIndex(AssemblyReference, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided assembly processor row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(AssemblyRefProcessorRow other)
        {
            return ProcessorId == other.ProcessorId && AssemblyReference == other.AssemblyReference;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is AssemblyRefProcessorRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) ProcessorId * 397) ^ (int) AssemblyReference;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({ProcessorId:X8}, {AssemblyReference:X8})";
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