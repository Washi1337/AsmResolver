using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    /// <summary>
    /// Represents a single raw metadata table in the metadata table stream (#~ or #-).
    /// </summary>
    public abstract class MetadataTable : FileSegment, IEnumerable<MetadataRow>
    {
        private bool _isReadOnly;

        /// <summary>
        /// Gets the type of rows this table contains.
        /// </summary>
        public abstract MetadataTokenType TokenType
        {
            get;
        }

        /// <summary>
        /// Gets the table stream the raw metadata table is stored in.
        /// </summary>
        public TableStream TableStream
        {
            get;
            internal set;
        }
        
        /// <summary>
        /// Gets the amount of rows the table has.
        /// </summary>
        public abstract int Count
        {
            get;
        }
        
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            internal set
            {
                if (_isReadOnly != value)
                {
                    _isReadOnly = value;
                    OnReadOnlyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the size of an index to a single row of the table.
        /// </summary>
        public IndexSize IndexSize
        {
            get { return Count > ushort.MaxValue ? IndexSize.Long : IndexSize.Short; }
        }

        /// <summary>
        /// Gets the amount of bytes a single row consists of in the table.
        /// </summary>
        public abstract uint ElementByteCount
        {
            get;
        }

        /// <summary>
        /// Gets a particular row using the given zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the row to get.</param>
        /// <returns>The row that is stored at this index.</returns>
        public abstract MetadataRow GetRow(int index);

        /// <summary>
        /// Tries to get a particular row given the zero-based index, 
        /// and returns a value indicating whether this was a success or not.
        /// </summary>
        /// <param name="index">The zero-based index of the row to get.</param>
        /// <param name="row">The row that was gotten, or null</param>
        /// <returns>True if the row was obtained successfully, false otherwise.</returns>
        public bool TryGetRow(int index, out MetadataRow row)
        {
            if (index >= 0 && index < Count)
            {
                row = GetRow(index);
                return true;
            }
            row = null;
            return false;
        }

        /// <summary>
        /// Gets a single row in a table by a key. This requires the table to be sorted.
        /// </summary>
        /// <param name="keyColumnIndex">The column number to get the key from.</param>
        /// <param name="key">The key to search.</param>
        /// <returns>The first row that contains the given key, or null if none was found.</returns>
        public MetadataRow GetRowByKey(int keyColumnIndex, uint key)
        {
            if (Count == 0)
                return null;

            int left = 0;
            int right = Count - 1;
           
            while (left <= right)
            {
                int m = (left + right) / 2;
                var currentRow = GetRow(m);
                uint currentKey = Convert.ToUInt32(currentRow.GetAllColumns()[keyColumnIndex]);

                if (currentKey > key)
                    right = m - 1;
                else if (currentKey < key)
                    left = m + 1;
                else
                    return currentRow;
            }

            return null;
        }

        /// <summary>
        /// Gets a single row in a table by a key. This requires the table to be sorted.
        /// </summary>
        /// <param name="keyColumnIndex">The column number to get the key from.</param>
        /// <param name="key">The key to search.</param>
        /// <returns>The first row that contains the given key, or null if none was found.</returns>
        public int GetRowIndexClosestToKey(int keyColumnIndex, uint key)
        {
            if (Count == 0)
                return -1;

            int left = 0;
            int right = Count - 1;
            int m = 0;

            while (left <= right)
            {
                m = (left + right) / 2;
                var row = GetRow(m);
                uint currentKey = Convert.ToUInt32(row.GetAllColumns()[keyColumnIndex]);

                if (currentKey > key)
                    right = m - 1;
                else if (currentKey < key)
                    left = m + 1;
                else
                    break;
            }

            while (m < Count - 1)
            {
                var nextRow = GetRow(m + 1);
                var nextKey = Convert.ToUInt32(nextRow.GetAllColumns()[keyColumnIndex]);
                if (nextKey > key)
                    return m;
                m++;
            }

            return m;
        }

        /// <summary>
        /// Gets a single row in a table by a key. This requires the table to be sorted.
        /// </summary>
        /// <param name="keyColumnIndex">The column number to get the key from.</param>
        /// <param name="key">The key to search.</param>
        /// <returns>The first row that contains the given key, or null if none was found.</returns>
        public MetadataRow GetRowClosestToKey(int keyColumnIndex, uint key)
        {
            int index = GetRowIndexClosestToKey(keyColumnIndex, key);
            return index != -1 ? GetRow(index) : null;
        }

        public abstract IMetadataMember GetMemberFromRow(MetadataImage image, MetadataRow row);

        /// <summary>
        /// Updates all the metadata tokens of each row in the table.
        /// </summary>
        public void UpdateTokens()
        {
            for (int i = 0; i < Count; i++)
                GetRow(i).MetadataToken = new MetadataToken(TokenType, (uint)(i + 1));
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(ElementByteCount * Count);
        }
        
        internal abstract void SetRowCount(uint capacity);

        internal abstract void SetReadingContext(ReadingContext readingContext);

        protected void AssertIsWriteable()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Table cannot be modified in read-only mode.");
        }

        protected void AssertIsReadOnly()
        {
            if (!IsReadOnly)
                throw new InvalidOperationException("Operation can only be performed if the table is in read-only mode.");
        }

        protected abstract IEnumerator<MetadataRow> GetRowsEnumerator();

        IEnumerator<MetadataRow> IEnumerable<MetadataRow>.GetEnumerator()
        {
            return GetRowsEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetRowsEnumerator();
        }

        protected virtual void OnReadOnlyChanged()
        {
        }
    }

    public abstract class MetadataTable<TRow> : MetadataTable, ICollection<TRow>
        where TRow: MetadataRow 
    {
        private readonly List<TRow> _rows;
        private ReadingContext _readingContext;

        protected MetadataTable()
        {
            _rows = new List<TRow>();
        }

        /// <summary>
        /// Gets or sets a single metadata row at the given zero-based index.
        /// </summary>
        /// <param name="index">The index of the row.</param>
        /// <returns>The row at the index.</returns>
        public TRow this[int index]
        {
            get { return (TRow) GetRow(index); }
            set
            {
                AssertIsWriteable();
                _rows[index] = value;
            }
        }

        public override int Count
        {
            get { return _rows.Count; }
        }

        /// <summary>
        /// Reads a single row using the given reading context.
        /// </summary>
        /// <param name="context">The reading context to use for reading the metadata row.</param>
        /// <param name="token">The token that is assigned to the metadata row to be read.</param>
        /// <returns>The metadata row that was read.</returns>
        protected abstract TRow ReadRow(ReadingContext context, MetadataToken token);

        /// <summary>
        /// Writes a single row to the output writing context.
        /// </summary>
        /// <param name="context">The writing context to use for writing the metadata row to.</param>
        /// <param name="row">The row to write.</param>
        protected abstract void WriteRow(WritingContext context, TRow row);

        /// <summary>
        /// Tries to get a particular row given the zero-based index, 
        /// and returns a value indicating whether this was a success or not.
        /// </summary>
        /// <param name="index">The zero-based index of the row to get.</param>
        /// <param name="row">The row that was gotten, or null</param>
        /// <returns>True if the row was obtained successfully, false otherwise.</returns>
        public bool TryGetMember(int index, out TRow row)
        {
            MetadataRow r;
            bool result = TryGetRow(index, out r);
            row = r as TRow;
            return result;
        }

        public override MetadataRow GetRow(int index)
        {
            lock (_rows)
            {
                if (_rows[index] == null && _readingContext != null)
                {
                    var context = _readingContext.CreateSubContext(
                        _readingContext.Reader.StartPosition + index * ElementByteCount);

                    var row = ReadRow(context, new MetadataToken(TokenType, (uint)(index + 1)));
                    row.IsReadOnly = IsReadOnly;
                    _rows[index] = row;
                }
            }
            return _rows[index];
        }

        public sealed override IMetadataMember GetMemberFromRow(MetadataImage image, MetadataRow row)
        {
            AssertIsReadOnly();
            IMetadataMember member;
            if (!image.TryGetCachedMember(row.MetadataToken, out member))
            {
                member = CreateMemberFromRow(image, (TRow) row);
                image.CacheMember(member);
            }
            return member;
        }

        protected abstract IMetadataMember CreateMemberFromRow(MetadataImage image, TRow row);

        protected void InsertRow(int index, TRow row)
        {
            AssertIsWriteable();
            _rows.Insert(index, row);
        }
        
        public virtual void Add(TRow item)
        {
            AssertIsWriteable();
            InsertRow(Count, item);
            item.MetadataToken = new MetadataToken(TokenType, (uint) Count);
        }

        public void Clear()
        {
            AssertIsWriteable();
            _rows.Clear();
        }

        public bool Contains(TRow item)
        {
            return _rows.Contains(item);
        }

        public void CopyTo(TRow[] array, int arrayIndex)
        {
            _rows.CopyTo(array, arrayIndex);
        }

        public bool Remove(TRow item)
        {
            AssertIsWriteable();
            if (_rows.Remove(item))
            {
                return true;
            }
            return false;
        }

        public new TRow GetRowByKey(int keyColumnIndex, uint key)
        {
            return (TRow) base.GetRowByKey(keyColumnIndex, key);
        }

        public new bool TryGetRow(int index, out TRow row)
        {
            MetadataRow mrow;
            bool result = base.TryGetRow(index, out mrow);
            row = mrow as TRow;
            return result;
        }
        
        public override void Write(WritingContext context)
        {
            foreach (var member in this)
                WriteRow(context, member);
        }

        protected override IEnumerator<MetadataRow> GetRowsEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TRow> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override void OnReadOnlyChanged()
        {
            base.OnReadOnlyChanged();
            foreach (var row in _rows)
            {
                if (row != null)
                    row.IsReadOnly = IsReadOnly;
            } 
        }

        internal override void SetRowCount(uint capacity)
        {
            AssertIsWriteable();
            _rows.AddRange(new TRow[capacity]);
        }

        internal override void SetReadingContext(ReadingContext readingContext)
        {
            _readingContext = readingContext;
            StartOffset = _readingContext.Reader.StartPosition;
        }
    }

    public abstract class SortedMetadataTable<TRow> : MetadataTable<TRow>
        where TRow : MetadataRow
    {
        protected SortedMetadataTable(int keyColumnIndex)
        {
            KeyColumnIndex = keyColumnIndex;
        }
        
        public int KeyColumnIndex
        {
            get;
            private set;
        }
        
        public override void Add(TRow item)
        {
            AssertIsWriteable();
            int index = GetRowIndexClosestToKey(KeyColumnIndex, (uint) item.GetAllColumns()[KeyColumnIndex]);
            item.MetadataToken = new MetadataToken(TokenType, (uint) (index + 2));
            InsertRow(index + 1, item);
        }
    }
}
