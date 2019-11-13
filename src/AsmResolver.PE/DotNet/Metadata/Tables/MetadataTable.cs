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
        public MetadataTable(TableLayout layout)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
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
        public void Add(TRow item)
        {
            throw new NotImplementedException();
        }

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
        protected virtual IList<TRow> GetRows()
        {
            return new List<TRow>();
        }

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