using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    // TODO: Implement a more granular lazy initialization.
    
    /// <summary>
    /// Provides a base implementation of a metadata table in the table stream of a managed executable file.
    /// </summary>
    /// <typeparam name="TRow">The type of rows that this table stores.</typeparam>
    public class MetadataTable<TRow> : IMetadataTable, ICollection<TRow>
        where TRow : struct, IMetadataRow
    {
        private IList<TRow> _items;

        /// <summary>
        /// Creates a new metadata table using the provided layout.
        /// </summary>
        /// <param name="layout">The layout of the table.</param>
        public MetadataTable(TableIndex tableIndex, TableLayout layout)
        {
            TableIndex = tableIndex;
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        }

        /// <summary>
        /// Gets the index of the table in the tables stream. 
        /// </summary>
        public TableIndex TableIndex
        {
            get;
        }
        
        /// <inheritdoc />
        public TableLayout Layout
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public IndexSize IndexSize => Count > 0xFFFF ? IndexSize.Long : IndexSize.Short;

        /// <inheritdoc cref="IMetadataTable" />
        public TRow this[int index]
        {
            get => Rows[index];
            set => Rows[index] = value;
        }

        /// <inheritdoc />
        IMetadataRow IMetadataTable.this[int index]
        {
            get => this[index];
            set => this[index] = (TRow) value;
        }

        /// <summary>
        /// Gets the internal list of rows that are stored in the metadata table.
        /// </summary>
        protected IList<TRow> Rows
        {
            get
            {
                if (_items is null)
                    Interlocked.CompareExchange(ref _items, GetRows(), null);
                return _items;
            }
        }

        /// <summary>
        /// Gets a value indicating the <see cref="Rows"/> property is initialized or not.
        /// </summary>
        protected bool IsInitialized => _items != null;

        /// <inheritdoc cref="ICollection{T}.Count" />
        public virtual int Count => Rows.Count;
        
        /// <inheritdoc />
        public bool IsReadOnly => false; // TODO: it might be necessary later to make this configurable.

        /// <inheritdoc />
        public object SyncRoot
        {
            get;
        } = new object();

        /// <inheritdoc />
        public bool IsSynchronized => false;

        /// <inheritdoc />
        public void Add(TRow item) => Rows.Add(item);

        /// <inheritdoc />
        public void Clear() => Rows.Clear();

        /// <inheritdoc />
        public bool Contains(TRow item) =>  Rows.Contains(item);
        
        /// <inheritdoc />
        public void CopyTo(TRow[] array, int arrayIndex) => Rows.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        void ICollection.CopyTo(Array array, int index) => ((ICollection) Rows).CopyTo(array, index);

        /// <inheritdoc />
        public bool Remove(TRow item) => Rows.Remove(item);

        /// <inheritdoc />
        public IEnumerator<TRow> GetEnumerator() => Rows.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Obtains all rows in the metadata table.
        /// </summary>
        /// <returns>The rows.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Rows"/> property.
        /// </remarks>
        protected virtual IList<TRow> GetRows() => new List<TRow>();

        /// <summary>
        /// Gets the contents of a row by its row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <returns>The row.</returns>
        public TRow GetByRid(uint rid) => this[(int) (rid - 1)];

        IMetadataRow IMetadataTable.GetByRid(uint rid) => GetByRid(rid);

        /// <summary>
        /// Attempts to get the contents of a row by its row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <param name="row">When successful, the read row.</param>
        /// <returns><c>true</c> if the RID existed an the row was obtained successfully, <c>false</c> otherwise.</returns>
        public bool TryGetByRid(uint rid, out TRow row)
        {
            if (rid >= 1 && rid <= Count)
            {
                row = GetByRid(rid);
                return true;
            }

            row = default;
            return false;
        }

        bool IMetadataTable.TryGetByRid(uint rid, out IMetadataRow row)
        {
            bool result = TryGetByRid(rid, out var r);
            row = r;
            return result;
        }

        /// <summary>
        /// Gets a single row in a table by a key. This requires the table to be sorted.
        /// </summary>
        /// <param name="keyColumnIndex">The column number to get the key from.</param>
        /// <param name="key">The key to search.</param>
        /// <param name="row">When this functions returns <c>true</c>, this parameter contains the first row that
        /// contains the given key.</param>
        /// <returns><c>true</c> if the row was found, <c>false</c> otherwise.</returns>
        public bool TryGetRowByKey(int keyColumnIndex, uint key, out TRow row)
        {
            row = default;
            if (Count == 0)
                return false;

            int left = 0;
            int right = Count - 1;
           
            while (left <= right)
            {
                int m = (left + right) / 2;
                var currentRow = Rows[m];
                uint currentKey = currentRow[keyColumnIndex];

                if (currentKey > key)
                {
                    right = m - 1;
                }
                else if (currentKey < key)
                {
                    left = m + 1;
                }
                else
                {
                    row = currentRow;
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        bool IMetadataTable.TryGetRowByKey(int keyColumnIndex, uint key, out IMetadataRow row)
        {
            bool result = TryGetRowByKey(keyColumnIndex, key, out var r);
            row = r;
            return result;
        }

        /// <summary>
        /// Sets the contents of a row by its row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <param name="row">The new contents of the row.</param>
        public void SetByRid(uint rid, TRow row) => this[(int) (rid - 1)] = row;

        void IMetadataTable.SetByRid(uint rid, IMetadataRow row) => SetByRid(rid, (TRow) row);

        /// <inheritdoc />
        public virtual void UpdateTableLayout(TableLayout layout)
        {
            for (int i = 0; i < Layout.Columns.Length; i++)
            {
                if (Layout.Columns[i].Name != layout.Columns[i].Name
                    || Layout.Columns[i].Type != layout.Columns[i].Type)
                {
                    throw new ArgumentException("New table layout does not match the original one.");
                }
            }

            Layout = layout;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            foreach (var row in Rows) 
                row.Write(writer, Layout);
        }
        
    }
}