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
        
        public AssemblyProcessorRow(uint processorId)
        {
            ProcessorId = processorId;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.AssemblyProcessor;

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
        
    }
}