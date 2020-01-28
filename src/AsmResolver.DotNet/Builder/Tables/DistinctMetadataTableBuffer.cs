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
        private readonly IDictionary<TRow, uint> _entryRids = new Dictionary<TRow, uint>();
        
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
                if (_entryRids.TryGetValue(value, out uint duplicateRid) && duplicateRid != rid)
                    throw new ArgumentException("Row is already present in the table.");

                var old = base[rid];
                base[rid] = value;
                
                _entryRids.Remove(old);
                _entryRids.Add(value, rid);
            }
        }

        /// <inheritdoc />
        public override uint Add(in TRow row)
        {
            if (!_entryRids.TryGetValue(row, out uint rid))
            {
                rid = base.Add(in row);
                _entryRids.Add(row, rid);
            }

            return rid;
        }
    }
}