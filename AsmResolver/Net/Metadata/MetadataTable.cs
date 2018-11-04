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
        
        /// <summary>
        /// Gets a value indicating whether the metadata table is locked or not.
        /// </summary>
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
            int middle = 0;

            while (left <= right)
            {
                middle = (left + right) / 2;
                var row = GetRow(middle);
                uint currentKey = Convert.ToUInt32(row.GetAllColumns()[keyColumnIndex]);

                if (currentKey > key)
                    right = middle - 1;
                else if (currentKey < key)
                    left = middle + 1;
                else
                    break;
            }

            return left > right ? right : middle;
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

        /// <summary>
        /// Interprets the provided metadata row and converts it to a higher level representation of the member.  
        /// </summary>
        /// <param name="image">The containing metadata image.</param>
        /// <param name="row">The metadata row to convert.</param>
        /// <returns>The higher level representation of the member.</returns>
        public abstract IMetadataMember GetMemberFromRow(MetadataImage image, MetadataRow row);

        /// <summary>
        /// Updates all the metadata tokens of each row in the table.
        /// </summary>
        public void UpdateTokens()
        {
            for (int i = 0; i < Count; i++)
                GetRow(i).MetadataToken = new MetadataToken(TokenType, (uint)(i + 1));
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength()
        {
            return (uint)(ElementByteCount * Count);
        }
        
        /// <summary>
        /// Overrides the row count of the table.
        /// </summary>
        /// <param name="capacity">The new capacity of the table.</param>
        internal abstract void SetRowCount(uint capacity);

        /// <summary>
        /// Overrides the underlying reader used to initialize the table.
        /// </summary>
        /// <param name="readingContext">The new reading context.</param>
        internal abstract void SetReadingContext(ReadingContext readingContext);

        /// <summary>
        /// Verifies the table is writable (i.e. not readonly).
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when the table is not writable.</exception>
        protected void AssertIsWritable()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Table cannot be modified in read-only mode.");
        }
        
        /// <summary>
        /// Verifies the table is in readonly mode.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when the table is in not in readonly mode.</exception>
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

    /// <summary>
    /// Represents a single raw metadata table in the metadata table stream (#~ or #-) containing
    /// elements of type <see cref="TRow"/>.
    /// </summary>
    /// <typeparam name="TRow">The type of the elements in the table.</typeparam>
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
            get => (TRow) GetRow(index);
            set
            {
                AssertIsWritable();
                _rows[index] = value;
            }
        }

        /// <inheritdoc cref="MetadataTable.Count" />
        public override int Count => _rows.Count;

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
            bool result = TryGetRow(index, out MetadataRow r);
            row = r as TRow;
            return result;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public sealed override IMetadataMember GetMemberFromRow(MetadataImage image, MetadataRow row)
        {
            AssertIsReadOnly();
            if (!image.TryGetCachedMember(row.MetadataToken, out var member))
            {
                member = CreateMemberFromRow(image, (TRow) row);
                image.CacheMember(member);
            }
            return member;
        }

        /// <summary>
        /// Creates a member from the provided metadata row.
        /// </summary>
        /// <param name="image">The containing image.</param>
        /// <param name="row">The row to convert.</param>
        /// <returns>The created member.</returns>
        protected abstract IMetadataMember CreateMemberFromRow(MetadataImage image, TRow row);

        /// <summary>
        /// Inserts a row at the given index.
        /// </summary>
        /// <param name="index">The index to insert into.</param>
        /// <param name="row">The row to insert.</param>
        protected void InsertRow(int index, TRow row)
        {
            AssertIsWritable();
            _rows.Insert(index, row);
        }

        /// <inheritdoc />
        public virtual void Add(TRow item)
        {
            AssertIsWritable();
            InsertRow(Count, item);
            item.MetadataToken = new MetadataToken(TokenType, (uint) Count);
        }

        /// <inheritdoc />
        public void Clear()
        {
            AssertIsWritable();
            _rows.Clear();
        }

        /// <inheritdoc />
        public bool Contains(TRow item)
        {
            return _rows.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(TRow[] array, int arrayIndex)
        {
            _rows.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(TRow item)
        {
            AssertIsWritable();
            if (_rows.Remove(item))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a single row in the table by a key. This requires the table to be sorted.
        /// </summary>
        /// <param name="keyColumnIndex">The column number to get the key from.</param>
        /// <param name="key">The key to search.</param>
        /// <returns>The first row that contains the given key, or null if none was found.</returns>
        public new TRow GetRowByKey(int keyColumnIndex, uint key)
        {
            return (TRow) base.GetRowByKey(keyColumnIndex, key);
        }

        /// <summary>
        /// Tries to get a particular row given the zero-based index, 
        /// and returns a value indicating whether this was a success or not.
        /// </summary>
        /// <param name="index">The zero-based index of the row to get.</param>
        /// <param name="row">The row that was gotten, or null</param>
        /// <returns>True if the row was obtained successfully, false otherwise.</returns>
        public bool TryGetRow(int index, out TRow row)
        {
            bool result = base.TryGetRow(index, out var mrow);
            row = mrow as TRow;
            return result;
        }

        /// <inheritdoc />
        public override void Write(WritingContext context)
        {
            foreach (var member in this)
                WriteRow(context, member);
        }

        /// <inheritdoc />
        protected override IEnumerator<MetadataRow> GetRowsEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<TRow> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Invoked when the readonly state of the table has changed.
        /// </summary>
        protected override void OnReadOnlyChanged()
        {
            base.OnReadOnlyChanged();
            foreach (var row in _rows)
            {
                if (row != null)
                    row.IsReadOnly = IsReadOnly;
            } 
        }

        /// <inheritdoc />
        internal override void SetRowCount(uint capacity)
        {
            AssertIsWritable();
            _rows.AddRange(new TRow[capacity]);
        }

        /// <inheritdoc />
        internal override void SetReadingContext(ReadingContext readingContext)
        {
            _readingContext = readingContext;
            StartOffset = _readingContext.Reader.StartPosition;
        }
    }

    /// <summary>
    /// Represents a metadata table that is sorted by one of the columns.
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    public abstract class SortedMetadataTable<TRow> : MetadataTable<TRow>
        where TRow : MetadataRow
    {
        protected SortedMetadataTable(int keyColumnIndex)
        {
            KeyColumnIndex = keyColumnIndex;
        }
        
        /// <summary>
        /// Gets the index of the column that is used as the key to sort the table. 
        /// </summary>
        public int KeyColumnIndex
        {
            get;
        }
        
        /// <inheritdoc />
        public override void Add(TRow item)
        {
            AssertIsWritable();
            int index = GetRowIndexClosestToKey(KeyColumnIndex, (uint) item.GetAllColumns()[KeyColumnIndex]);
            item.MetadataToken = new MetadataToken(TokenType, (uint) (index + 2));
            
            InsertRow(index + 1, item);
        }
    }
}
