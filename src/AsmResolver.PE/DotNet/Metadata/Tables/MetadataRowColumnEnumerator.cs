using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides a mechanism for enumerating all column cells in a row of a metadata table.
    /// </summary>
    public struct MetadataRowColumnEnumerator : IEnumerator<uint>
    {
        private readonly IMetadataRow _row;
        private int _index;

        /// <summary>
        /// Creates a new metadata row column enumerator.
        /// </summary>
        /// <param name="row"></param>
        public MetadataRowColumnEnumerator(IMetadataRow row)
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
