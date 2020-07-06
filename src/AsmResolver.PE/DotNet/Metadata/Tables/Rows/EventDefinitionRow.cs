using System;
using System.Collections;
using System.Collections.Generic;

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
        
        /// <summary>
        /// Creates a new row for the event definition metadata table.
        /// </summary>
        /// <param name="attributes">The attributes associated to the event definition.</param>
        /// <param name="name">The index into the #Strings stream referencing the name of the event.</param>
        /// <param name="eventType">The TypeDefOrRef index (an index into either the TypeRef, TypeDef or TypeSpec table)
        /// indicating the type of the event.</param>
        public EventDefinitionRow(EventAttributes attributes, uint name, uint eventType)
        {
            Attributes = attributes;
            Name = name;
            EventType = eventType;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Event;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => Name,
            2 => EventType,
            _ => throw new IndexOutOfRangeException()
        };

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

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(EventType, (IndexSize) layout.Columns[2].Size);
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