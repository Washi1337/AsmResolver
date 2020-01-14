using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Tables
{
    /// <summary>
    /// Provides an implementation of a metadata table buffer, that sorts all rows in ascending order by a primary column.
    /// </summary>
    /// <typeparam name="TRow">The type of rows that this buffer contains.</typeparam>
    public class SortedMetadataTableBuffer<TRow> : IMetadataTableBuffer<TRow> 
        where TRow : struct, IMetadataRow
    {
        private readonly TableLayout _layout;
        private readonly int _primaryKeyColumn;
        private readonly List<Entry> _entries = new List<Entry>();

        /// <summary>
        /// Creates a new instance of a sorted metadata table buffer.
        /// </summary>
        /// <param name="layout">The layout of the final table.</param>
        /// <param name="primaryKeyColumn">The index of the column to use for sorting.</param>
        public SortedMetadataTableBuffer(TableLayout layout, int primaryKeyColumn)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _primaryKeyColumn = primaryKeyColumn;
        }

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <inheritdoc />
        public MetadataRowHandle Add(TRow row)
        {
            var handle = new MetadataRowHandle(_entries.Count + 1);
            _entries.Add(new Entry(row, handle));
            return handle;
        }

        /// <inheritdoc />
        public IConstructedTableInfo<TRow> CreateTable()
        {
            var comparer = new EntryComparer(_primaryKeyColumn);
            _entries.Sort(comparer);
            
            var table = new MetadataTable<TRow>(_layout);
            var mapping = new Dictionary<MetadataRowHandle, uint>();

            for (uint rid = 1; rid <= _entries.Count; rid++)
            {
                var entry = _entries[(int) (rid - 1)];
                table.Add(entry.Row);
                mapping[entry.Handle] = rid;
            }

            return new MappedConstructedTableInfo<TRow>(table, mapping);
        }

        private struct Entry
        {
            public Entry(TRow row, MetadataRowHandle handle)
            {
                Row = row;
                Handle = handle;
            }
            
            public TRow Row
            {
                get;
            }

            public MetadataRowHandle Handle
            {
                get;
            }
            
            #if DEBUG
            public override string ToString()
            {
                return $"{nameof(Row)}: {Row}, {nameof(Handle)}: {Handle}";
            }
            #endif
        }
        
        private struct EntryComparer : IComparer<Entry>
        {
            private readonly int _primaryColumnIndex;

            public EntryComparer(int primaryColumnIndex)
            {
                _primaryColumnIndex = primaryColumnIndex;
            }
            
            public int Compare(Entry x, Entry y)
            {
                return x.Row[_primaryColumnIndex].CompareTo(y.Row[_primaryColumnIndex]);
            }
        }
    }
}