using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Decorates a metadata table buffer with a filter that removes all duplicated rows from the buffer.
    /// </summary>
    /// <typeparam name="TRow">The type of rows to store.</typeparam>
    public class DistinctMetadataTableBuffer<TRow> : IMetadataTableBuffer<TRow>
        where TRow : struct, IMetadataRow
    {
        private readonly Dictionary<TRow, MetadataToken> _entries = new();
        private readonly IMetadataTableBuffer<TRow> _underlyingBuffer;

        /// <summary>
        /// Creates a new distinct metadata table buffer decorator.
        /// </summary>
        /// <param name="underlyingBuffer">The underlying table buffer.</param>
        public DistinctMetadataTableBuffer(IMetadataTableBuffer<TRow> underlyingBuffer)
        {
            _underlyingBuffer = underlyingBuffer ?? throw new ArgumentNullException(nameof(underlyingBuffer));
        }

        /// <inheritdoc />
        public int Count => _underlyingBuffer.Count;

        /// <inheritdoc />
        public TRow this[uint rid]
        {
            get => _underlyingBuffer[rid];
            set
            {
                if (_entries.TryGetValue(value, out var duplicateToken) && duplicateToken.Rid != rid)
                    throw new ArgumentException("Row is already present in the table.");

                var old = _underlyingBuffer[rid];
                _underlyingBuffer[rid] = value;

                _entries.Remove(old);
                _entries.Add(value, rid);
            }
        }

        /// <inheritdoc />
        public void EnsureCapacity(int capacity)
        {
            _underlyingBuffer.EnsureCapacity(capacity);

#if NETSTANDARD2_1_OR_GREATER
            _entries.EnsureCapacity(capacity);
#endif
        }

        /// <inheritdoc />
        public ref TRow GetRowRef(uint rid) => ref _underlyingBuffer.GetRowRef(rid);

        /// <inheritdoc />
        public MetadataToken Add(in TRow row) => Add(row, false);

        /// <inheritdoc />
        public MetadataToken Insert(uint rid, in TRow row) => Insert(rid, row, false);

        /// <summary>
        /// Inserts a row into the metadata table at the provided row identifier.
        /// </summary>
        /// <param name="rid">The row identifier.</param>
        /// <param name="row">The row to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.</param>
        /// <returns>The metadata token that this row was assigned to.</returns>
        public MetadataToken Insert(uint rid, in TRow row, bool allowDuplicates)
        {
            if (!_entries.TryGetValue(row, out var token))
            {
                token = _underlyingBuffer.Insert(rid, in row);
                _entries.Add(row, token);
            }
            else if (allowDuplicates)
            {
                token = _underlyingBuffer.Insert(rid, in row);
            }

            return token;
        }

        /// <summary>
        /// Adds a row to the metadata table buffer.
        /// </summary>
        /// <param name="row">The row to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.</param>
        /// <returns>The metadata token that this row was assigned to.</returns>
        public MetadataToken Add(in TRow row, bool allowDuplicates)
        {
            if (!_entries.TryGetValue(row, out var token))
            {
                token = _underlyingBuffer.Add(in row);
                _entries.Add(row, token);
            }
            else if (allowDuplicates)
            {
                token = _underlyingBuffer.Add(in row);
            }

            return token;
        }

        /// <inheritdoc />
        public void FlushToTable() => _underlyingBuffer.FlushToTable();

        /// <inheritdoc />
        public void Clear()
        {
            _underlyingBuffer.Clear();
            _entries.Clear();
        }
    }
}
