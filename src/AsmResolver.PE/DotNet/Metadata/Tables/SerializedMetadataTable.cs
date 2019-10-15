using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    public class SerializedMetadataTable<TRow> : MetadataTable<TRow> 
        where TRow : struct, IMetadataRow
    {
        public delegate TRow ReadRowDelegate(IBinaryStreamReader reader, TableLayout layout);

        private readonly IBinaryStreamReader _reader;
        private readonly TableLayout _layout;
        private readonly int _rowCount;
        private readonly ReadRowDelegate _readRow;

        public SerializedMetadataTable(IBinaryStreamReader reader, TableLayout layout, ReadRowDelegate readRow)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _rowCount = (int) (reader.Length / layout.RowSize);
            _readRow = readRow ?? throw new ArgumentNullException(nameof(readRow));
        }

        /// <inheritdoc />
        public override int Count => IsInitialized ? Rows.Count : _rowCount; 

        /// <inheritdoc />
        protected override IList<TRow> GetRows()
        {
            var result = new List<TRow>();

            for (int i = 0; i < _rowCount; i++) 
                result.Add(_readRow(_reader, _layout));

            return result;
        }
        
    }
}