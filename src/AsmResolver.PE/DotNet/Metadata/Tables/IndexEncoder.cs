using System;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
 /// <summary>
    /// Provides a mechanism for translating between metadata tokens and coded indices.
    /// </summary>
    public class IndexEncoder
    {
        private readonly DotNet.Metadata.TablesStream _tableStream;
        private readonly TableIndex[] _tables;
        private readonly int _tableIndexBitCount;
        private readonly int _tableIndexBitMask;
        private readonly int _maxSmallTableMemberCount;

        /// <summary>
        /// Creates a new index encoder.
        /// </summary>
        /// <param name="tableStream">The tables stream containing the tables the encoder targets.</param>
        /// <param name="tables">The table indices to encode for.</param>
        public IndexEncoder(DotNet.Metadata.TablesStream tableStream, params TableIndex[] tables)
        {
            _tableStream = tableStream ?? throw new ArgumentNullException(nameof(tableStream));
            _tables = tables ?? throw new ArgumentNullException(nameof(tables));

            _tableIndexBitCount = (int)Math.Ceiling(Math.Log(tables.Length, 2));
            _tableIndexBitMask = (int)(Math.Pow(2, _tableIndexBitCount) - 1);
            _maxSmallTableMemberCount = ushort.MaxValue >> _tableIndexBitCount;

        }

        /// <summary>
        /// Gets the size of the indices encoded by the index encoder.
        /// </summary>
        public IndexSize IndexSize
        {
            get
            {
                if (_tableStream.ForceLargeColumns)
                    return IndexSize.Long;

                uint maxCount = 0;
                foreach (var table in _tables)
                    maxCount = Math.Max(maxCount, _tableStream.GetTableRowCount(table));

                return maxCount > _maxSmallTableMemberCount
                    ? IndexSize.Long
                    : IndexSize.Short;
            }
        }

        /// <summary>
        /// Translates a metadata token to its corresponding coded index.
        /// </summary>
        /// <param name="token">The metadata token to encode.</param>
        /// <returns>The coded index.</returns>
        /// <exception cref="ArgumentException">
        /// Occurs when the provided metadata token is part of an unsupported metadata table.
        /// </exception>
        public uint EncodeToken(MetadataToken token)
        {
            int index = Array.IndexOf(_tables, token.Table);
            if (index == -1)
                throw new ArgumentException("Table is not supported by this encoder.", nameof(token));

            return (token.Rid << _tableIndexBitCount) | (uint)index;
        }

        /// <summary>
        /// Translates a coded index to its corresponding metadata token.
        /// </summary>
        /// <param name="codedIndex">The coded index to decode.</param>
        /// <returns>The decoded metadata token.</returns>
        public MetadataToken DecodeIndex(uint codedIndex)
        {
            long tableIndex = codedIndex & _tableIndexBitMask;
            uint rowIndex = codedIndex >> _tableIndexBitCount;

            return new MetadataToken(tableIndex >= _tables.Length
                    ? TableIndex.Module
                    : _tables[tableIndex],
                rowIndex);
        }
    }
}
