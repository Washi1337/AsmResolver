using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Represents a metadata stream buffer that sorts all added rows by one or two primary columns.
    /// </summary>
    /// <typeparam name="TKey">The type of members that are assigned new metadata rows.</typeparam>
    /// <typeparam name="TRow">The type of rows to store.</typeparam>
    public class SortedMetadataTableBuffer<TKey, TRow> : ISortedMetadataTableBuffer<TKey, TRow>
        where TKey : notnull
        where TRow : struct, IMetadataRow
    {
        /// <summary>
        /// The entries that this table will contain.
        /// - Key: The original key to be able to assign metadata tokens easily after sorting.
        /// - Row: The metadata row that was constructed for this key.
        /// - InputIndex: An index to ensure a stable sort.
        /// </summary>
        private readonly List<(TKey Key, TRow Row, int InputIndex)> _entries = new();

        private readonly Dictionary<TKey, MetadataToken> _newTokens = new();
        private readonly MetadataTable<TRow> _table;
        private readonly EntryComparer _comparer;

        /// <summary>
        /// Creates a new metadata stream buffer that is sorted by a single primary column.
        /// </summary>
        /// <param name="table">The underlying table to flush to.</param>
        /// <param name="primaryColumn">The index of the primary column to use as a sorting key.</param>
        public SortedMetadataTableBuffer(MetadataTable<TRow> table, int primaryColumn)
            : this(table, primaryColumn, primaryColumn)
        {
        }

        /// <summary>
        /// Creates a new metadata stream buffer that is sorted by a primary and a secondary column.
        /// </summary>
        /// <param name="table">The underlying table to flush to.</param>
        /// <param name="primaryColumn">The index of the primary column to use as a sorting key.</param>
        /// <param name="secondaryColumn">The index of the secondary column to use as a sorting key.</param>
        public SortedMetadataTableBuffer(MetadataTable<TRow> table, int primaryColumn, int secondaryColumn)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _comparer = new EntryComparer(primaryColumn, secondaryColumn);
        }

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <inheritdoc />
        public void Add(TKey originalKey, in TRow row)
        {
            _entries.Add((originalKey, row, _entries.Count));
        }

        /// <inheritdoc />
        public IEnumerable<TKey> GetMembers() => _entries.Select(entry => entry.Key);

        /// <inheritdoc />
        public void Sort()
        {
            _entries.Sort(_comparer);

            for (uint rid = 1; rid <= _entries.Count; rid++)
            {
                var member = _entries[(int) (rid - 1)].Key;
                _newTokens[member] = new MetadataToken(_table.TableIndex, rid);
            }
        }

        /// <inheritdoc />
        public MetadataToken GetNewToken(TKey member) => _newTokens[member];

        /// <inheritdoc />
        public void FlushToTable()
        {
            Sort();

            _table.Clear();
            foreach (var row in _entries)
                _table.Add(row.Row);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _entries.Clear();
            _table.Clear();
        }

        private sealed class EntryComparer : IComparer<(TKey Key, TRow Row, int InputIndex)>
        {
            private readonly int _primaryColumn;
            private readonly int _secondaryColumn;

            public EntryComparer(int primaryColumn, int secondaryColumn)
            {
                _primaryColumn = primaryColumn;
                _secondaryColumn = secondaryColumn;
            }

            public int Compare((TKey Key, TRow Row, int InputIndex) x, (TKey Key, TRow Row, int InputIndex) y)
            {
                int result = x.Row[_primaryColumn].CompareTo(y.Row[_primaryColumn]);
                if (result == 0)
                    result = x.Row[_secondaryColumn].CompareTo(y.Row[_secondaryColumn]);
                if (result == 0)
                    result = x.InputIndex.CompareTo(y.InputIndex);
                return result;
            }
        }
    }
}
