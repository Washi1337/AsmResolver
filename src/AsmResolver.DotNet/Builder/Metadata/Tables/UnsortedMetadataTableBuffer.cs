using System;
using System.Collections.Generic;
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
        private readonly List<TRow> _entries = new List<TRow>();
        private readonly MetadataTable<TRow> _table;

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
            set => _entries[(int) (rid - 1)] = value;
        }

        /// <inheritdoc />
        public virtual MetadataToken Add(in TRow row)
        {
            _entries.Add(row);
            return new MetadataToken(_table.TableIndex, (uint) _entries.Count);
        }

        /// <inheritdoc />
        public void FlushToTable()
        {
            foreach (var row in _entries)
                _table.Add(row);
        }

        /// <inheritdoc />
        public void Clear() => _entries.Clear();
    }
}