using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Metadata.Tables
{
    /// <summary>
    /// Represents a metadata stream buffer that sorts all added rows by one or two primary columns.
    /// </summary>
    /// <typeparam name="TRow">The type of rows to store.</typeparam>
    public class SortedMetadataTableBuffer<TRow> : IMetadataTableBuffer<TRow> 
        where TRow : struct, IMetadataRow
    {
        private readonly List<TRow> _entries = new List<TRow>();
        private readonly MetadataTable<TRow> _table;
        private readonly IComparer<TRow> _comparer;
        
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
            _comparer = new RowComparer(primaryColumn, secondaryColumn);
        }

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <inheritdoc />
        public TRow this[uint rid]
        {
            get => _entries[(int) (rid - 1)];
            set => _entries[(int) (rid - 1)] = value;
        }

        /// <inheritdoc />
        public MetadataToken Add(in TRow row, uint originalRid)
        {
            _entries.Add(row);
            return new MetadataToken(_table.TableIndex, (uint) _entries.Count);
        }

        /// <inheritdoc />
        public void FlushToTable()
        {
            _entries.Sort(_comparer);
            foreach (var row in _entries)
                _table.Add(row);
        }

        private sealed class RowComparer : IComparer<TRow>
        {
            private readonly int _primaryColumn;
            private readonly int _secondaryColumn;

            public RowComparer(int primaryColumn, int secondaryColumn)
            {
                _primaryColumn = primaryColumn;
                _secondaryColumn = secondaryColumn;
            }
            
            public int Compare(TRow x, TRow y)
            {
                int result = x[_primaryColumn].CompareTo(y[_primaryColumn]);
                if (result == 0)
                    result = x[_secondaryColumn].CompareTo(y[_secondaryColumn]);
                return result;
            }
        }
    }
}