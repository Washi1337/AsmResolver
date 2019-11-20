using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    public struct MetadataRowColumnEnumerator<TRow> : IEnumerator<uint>
        where TRow : IMetadataRow
    {
        private TRow _row;
        private int _index;

        public MetadataRowColumnEnumerator(TRow row)
        {
            _row = row;
            _index = -1;
        }

        /// <inheritdoc />
        public uint Current => _index >= 0 && _index < _row.Count ? _row[_index] : default;

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_index < _row.Count - 1)
            {
                _index++;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _index = 0;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
        
    }
}