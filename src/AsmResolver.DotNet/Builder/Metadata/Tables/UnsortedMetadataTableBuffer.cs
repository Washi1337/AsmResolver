using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Metadata.Tables
{
    /// <summary>
    /// Represents a metadata stream buffer that adds every new row at the end of the table, without any further
    /// processing or reordering of the rows.
    /// </summary>
    /// <typeparam name="TRow">The type of rows to store.</typeparam>
    public class UnsortedMetadataTableBuffer<TRow> : IMetadataTableBuffer<TRow>
        where TRow : struct, IMetadataRow
    {
        private readonly RefList<TRow> _entries = new();
        private readonly BitList _available = new();
        private readonly MetadataTable<TRow> _table;
        private uint _currentRid;

        /// <summary>
        /// Creates a new unsorted metadata table buffer.
        /// </summary>
        /// <param name="table">The underlying table to flush to.</param>
        public UnsortedMetadataTableBuffer(MetadataTable<TRow> table)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
        }

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <inheritdoc />
        public virtual TRow this[uint rid]
        {
            get => _entries[(int) (rid - 1)];
            set
            {
                _entries[(int) (rid - 1)] = value;
                _available[(int) (rid - 1)] = false;
            }
        }

        /// <inheritdoc />
        public void EnsureCapacity(int capacity)
        {
            if (_entries.Capacity < capacity)
                _entries.Capacity = capacity;
        }

        /// <inheritdoc />
        public ref TRow GetRowRef(uint rid) => ref _entries.GetElementRef((int)(rid - 1));

        /// <inheritdoc />
        public virtual MetadataToken Add(in TRow row)
        {
            // Move over unavailable slots.
            while (_currentRid < _available.Count && !_available[(int) _currentRid])
                _currentRid++;

            // If we moved over all entries, we're adding to the end.
            if (_currentRid == _entries.Count)
            {
                _currentRid++;
            }

            return Insert(_currentRid++, row);
        }

        /// <inheritdoc />
        public MetadataToken Insert(uint rid, in TRow row)
        {
            EnsureRowsAllocated(rid);

            var token = new MetadataToken(_table.TableIndex, rid);

            if (!_available[(int) (rid - 1)])
            {
                if (EqualityComparer<TRow>.Default.Equals(row, _entries[(int) (rid - 1)]))
                    return token;

                throw new InvalidOperationException($"Token 0x{token.ToString()} is already in use.");
            }

            this[rid] = row;
            return token;
        }

        private void EnsureRowsAllocated(uint rid)
        {
            while (_entries.Count < rid)
            {
                _entries.Add(default);
                _available.Add(true);
            }
        }

        /// <inheritdoc />
        public void FlushToTable()
        {
            if (_table.Capacity < _entries.Count)
                _table.Capacity = _entries.Count;

            foreach (var row in _entries)
                _table.Add(row);
        }

        /// <inheritdoc />
        public void Clear() => _entries.Clear();
    }
}
