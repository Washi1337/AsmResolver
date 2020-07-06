using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the assembly processor metadata table.
    /// </summary>
    public readonly struct AssemblyProcessorRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single assembly processor row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the assembly processor table.</param>
        /// <returns>The row.</returns>
        public static AssemblyProcessorRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new AssemblyProcessorRow(reader.ReadUInt32());
        }
        
        /// <summary>
        /// Creates a new row for the assembly processor metadata table.
        /// </summary>
        /// <param name="processorId">The processor identifier the assembly is targeting.</param>
        public AssemblyProcessorRow(uint processorId)
        {
            ProcessorId = processorId;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.AssemblyProcessor;

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => ProcessorId,
            _ => throw new IndexOutOfRangeException()
        };
        
        /// <summary>
        /// Gets the processor identifier the assembly is targeting.
        /// </summary>
        public uint ProcessorId
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(ProcessorId);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided assembly processor row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(AssemblyProcessorRow other)
        {
            return ProcessorId == other.ProcessorId;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is AssemblyProcessorRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) ProcessorId;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({ProcessorId:X8})";
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