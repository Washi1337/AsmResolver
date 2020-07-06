using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the event map metadata table.
    /// </summary>
    public readonly struct EventMapRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single event map row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the event map table.</param>
        /// <returns>The row.</returns>
        public static EventMapRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new EventMapRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <summary>
        /// Creates a new row for the event map metadata table.
        /// </summary>
        /// <param name="parent">The index into the TypeDef table that this mapping is associating to an event list.</param>
        /// <param name="eventList">The index into the Event table indicating the first event that is defined in the event list.</param>
        public EventMapRow(uint parent, uint eventList)
        {
            Parent = parent;
            EventList = eventList;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.EventMap;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Parent,
            1 => EventList,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets an index into the TypeDef table that this mapping is associating to an event list.
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <summary>
        /// Gets an index into the Event table indicating the first event that is defined in the event list.
        /// </summary>
        public uint EventList
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(EventList, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided event map row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(EventMapRow other)
        {
            return Parent == other.Parent && EventList == other.EventList;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EventMapRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Parent * 397) ^ (int) EventList;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Parent:X8}, {EventList:X8})";
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