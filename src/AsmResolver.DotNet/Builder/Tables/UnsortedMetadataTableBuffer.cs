using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Tables
{
    /// <summary>
    /// Provides an implementation of a metadata table buffer, that does not apply any special ordering on the added rows.
    /// </summary>
    /// <typeparam name="TRow">The type of rows that this buffer contains.</typeparam>
    public class UnsortedMetadataTableBuffer<TRow> : IMetadataTableBuffer<TRow>
        where TRow : struct, IMetadataRow
    {
        private readonly TableLayout _layout;
        private readonly List<TRow> _rows = new List<TRow>();
        
        /// <summary>
        /// Creates a new instance of an unsorted metadata table buffer.
        /// </summary>
        /// <param name="layout">The layout of the final table.</param>
        public UnsortedMetadataTableBuffer(TableLayout layout)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        }

        /// <inheritdoc />
        public int Count => _rows.Count;

        /// <inheritdoc />
        public MetadataRowHandle Add(TRow row)
        {
            _rows.Add(row);
            return new MetadataRowHandle(_rows.Count);
        }

        /// <inheritdoc />
        public IConstructedTableInfo<TRow> CreateTable()
        {
            var table = new MetadataTable<TRow>(_layout);

            foreach (var row in _rows)
                table.Add(row);

            return new SimpleConstructedTableInfo<TRow>(table);
        }
    }
}