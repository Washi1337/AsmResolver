using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    public class SerializedMetadataTable<TRow> : MetadataTable<TRow> 
        where TRow : struct, IMetadataRow
    {
        public delegate TRow ReadRowDelegate(IBinaryStreamReader reader, TableLayout layout);
        
        public delegate TRow ReadRowExtendedDelegate(IBinaryStreamReader reader,
            TableLayout layout,
            ISegmentReferenceResolver resolver);

        private readonly IBinaryStreamReader _reader;
        private readonly TableLayout _originalLayout;
        private readonly int _rowCount;
        private readonly ReadRowDelegate _readRow;

        public SerializedMetadataTable(IBinaryStreamReader reader, TableLayout originalLayout, ReadRowDelegate readRow)
            : base(originalLayout)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _originalLayout = originalLayout;
            _rowCount = (int) (reader.Length / originalLayout.RowSize);
            _readRow = readRow ?? throw new ArgumentNullException(nameof(readRow));
        }

        public SerializedMetadataTable(IBinaryStreamReader reader, TableLayout originalLayout,
            ReadRowExtendedDelegate readRow, ISegmentReferenceResolver referenceResolver)
            : this(reader, originalLayout, (r, l) => readRow(r, l, referenceResolver))
        {
        }

        /// <inheritdoc />
        public override int Count => IsInitialized ? Rows.Count : _rowCount; 

        /// <inheritdoc />
        protected override IList<TRow> GetRows()
        {
            var result = new List<TRow>();

            for (int i = 0; i < _rowCount; i++) 
                result.Add(_readRow(_reader, _originalLayout));

            return result;
        }
        
    }
}