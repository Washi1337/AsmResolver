using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Metadata.Tables
{
    /// <summary>
    /// Decorates a metadata table buffer with a filter that removes all duplicated rows from the buffer.  
    /// </summary>
    /// <typeparam name="TRow">The type of rows to store.</typeparam>
    public class DistinctMetadataTableBuffer<TRow> : IMetadataTableBuffer<TRow>
        where TRow : struct, IMetadataRow
    {
        private readonly IMetadataTableBuffer<TRow> _underlyingBuffer;
        private readonly IDictionary<TRow, MetadataToken> _entries = new Dictionary<TRow, MetadataToken>();
        
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
        public MetadataToken Add(in TRow row)
        {
            if (!_entries.TryGetValue(row, out var token))
            {
                token = _underlyingBuffer.Add(in row);
                _entries.Add(row, token);
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