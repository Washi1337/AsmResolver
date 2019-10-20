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
    /// Represents a single row in the event map metadata table.
    /// </summary>
    public readonly struct EventDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single event definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the event definition table.</param>
        /// <returns>The row.</returns>
        public static EventDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new EventDefinitionRow(
                (EventAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }
        
        public EventDefinitionRow(EventAttributes attributes, uint name, uint eventType)
        {
            Attributes = attributes;
            Name = name;
            EventType = eventType;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Event;

        /// <summary>
        /// Gets the attributes associated to the event definition. 
        /// </summary>
        public EventAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings stream referencing the name of the event.
        /// </summary>
        /// <remarks>
        /// This value should always index a non-empty string.
        /// </remarks>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Gets a TypeDefOrRef index (an index into either the TypeRef, TypeDef or TypeSpec table) indicating the
        /// type of the event.
        /// </summary>
        public uint EventType
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided event definitino row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(EventDefinitionRow other)
        {
            return Attributes == other.Attributes && Name == other.Name && EventType == other.EventType;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EventDefinitionRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) EventType;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) Attributes:X4}, {Name:X8}, {EventType:X8})";
        }
        
    }
}