using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Tables
{
    /// <summary>
    /// Represents an unsorted metadata table buffer that only consists of distinct metadata rows, and will reuse the
    /// same row and assigned RID when a duplicate is added. 
    /// </summary>
    /// <typeparam name="TRow">The type of rows to store.</typeparam>
    public class DistinctMetadataTableBuffer<TRow> : UnsortedMetadataTableBuffer<TRow>
        where TRow : struct, IMetadataRow
    {
        private readonly IDictionary<TRow, MetadataToken> _entries = new Dictionary<TRow, MetadataToken>();
        
        /// <summary>
        /// Creates a new distinct metadata table buffer.
        /// </summary>
        // <param name="table">The underlying table to flush to.</param>
        public DistinctMetadataTableBuffer(MetadataTable<TRow> table)
            : base(table)
        {
        }

        /// <inheritdoc />
        public override TRow this[uint rid]
        {
            get => base[rid];
            set
            {
                if (_entries.TryGetValue(value, out var duplicateToken) && duplicateToken.Rid != rid)
                    throw new ArgumentException("Row is already present in the table.");

                var old = base[rid];
                base[rid] = value;
                
                _entries.Remove(old);
                _entries.Add(value, rid);
            }
        }

        /// <inheritdoc />
        public override MetadataToken Add(in TRow row, uint originalRid)
        {
            if (!_entries.TryGetValue(row, out var token))
            {
                token = base.Add(in row, originalRid);
                _entries.Add(row, token);
            }

            return token;
        }
    }
}