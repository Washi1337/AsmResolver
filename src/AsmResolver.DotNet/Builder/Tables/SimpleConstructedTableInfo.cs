using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Tables
{
    internal struct SimpleConstructedTableInfo<TRow> : IConstructedTableInfo<TRow>
        where TRow : struct, IMetadataRow
    {
        public SimpleConstructedTableInfo(MetadataTable<TRow> serializedTable)
        {
            ConstructedTable = serializedTable; 
        }
        
        public MetadataTable<TRow> ConstructedTable
        {
            get;
        }

        public uint GetRid(MetadataRowHandle handle) => (uint) handle.Id;
    }
}