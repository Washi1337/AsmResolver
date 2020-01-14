using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Tables
{
    internal struct MappedConstructedTableInfo<TRow> : IConstructedTableInfo<TRow>
        where TRow : struct, IMetadataRow
    {
        private readonly IDictionary<MetadataRowHandle, uint> _ridMapping;

        public MappedConstructedTableInfo(MetadataTable<TRow> serializedTable, IDictionary<MetadataRowHandle, uint> ridMapping)
        {
            ConstructedTable = serializedTable;
            _ridMapping = ridMapping;
        }
        
        public MetadataTable<TRow> ConstructedTable
        {
            get;
        }

        public uint GetRid(MetadataRowHandle handle) => _ridMapping[handle];
    }
}