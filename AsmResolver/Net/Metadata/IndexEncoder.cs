using System;
using System.Linq;

namespace AsmResolver.Net.Metadata
{
    public class IndexEncoder
    {
        private readonly TableStream _tableStream;
        private readonly MetadataTokenType[] _tables;
        private readonly int _tableIndexBitCount;
        private readonly int _tableIndexBitMask;
        private readonly int _maxSmallTableMemberCount;

        public IndexEncoder(TableStream tableStream, params MetadataTokenType[] tables)
        {
            if (tableStream == null)
                throw new ArgumentNullException("tableStream");
            if (tables == null)
                throw new ArgumentNullException("tables");
            _tableStream = tableStream;
            _tables = tables;

            _tableIndexBitCount = (int)Math.Ceiling(Math.Log(tables.Length, 2));
            _tableIndexBitMask = (int)(Math.Pow(2, _tableIndexBitCount) - 1);
            _maxSmallTableMemberCount = ushort.MaxValue >> _tableIndexBitCount;
            
        }

        public IndexSize IndexSize
        {
            get
            {
                int maxCount =
                    (from table in _tables
                     where table != TableStream.NotUsed
                     select _tableStream.GetTable(table).Count).Max();

                return maxCount > _maxSmallTableMemberCount ? IndexSize.Long : IndexSize.Short;
            }
        }

        public uint EncodeToken(MetadataToken token)
        {
            var index = Array.IndexOf(_tables, token.TokenType);
            if (index == -1)
                throw new ArgumentException("Table is not supported by this encoder.", "token");

            return (token.Rid << _tableIndexBitCount) | (uint)index;
        }

        public MetadataToken DecodeIndex(uint codedIndex)
        {
            var tableIndex = codedIndex & _tableIndexBitMask;
            var rowIndex = codedIndex >> _tableIndexBitCount;
            return new MetadataToken(tableIndex >= _tables.Length ? MetadataTokenType.Module : _tables[tableIndex],
                rowIndex);
        }
    }
}
