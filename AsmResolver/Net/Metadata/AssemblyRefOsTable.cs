using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyRefOsTable : MetadataTable<MetadataRow<uint, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyRefOs; }
        }

        public override uint ElementByteCount
        {
            get {
                return sizeof(uint) +
                       sizeof(uint) +
                       sizeof(uint) +
                       (uint) TableStream.GetTable(MetadataTokenType.AssemblyRef).IndexSize;
            }
        }

        protected override MetadataRow<uint, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint, uint, uint>(token)
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadUInt32(),
                Column3 = reader.ReadUInt32(),
                Column4 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.AssemblyRef).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32(row.Column1);
            writer.WriteUInt32(row.Column2);
            writer.WriteUInt32(row.Column3);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.AssemblyRef).IndexSize, row.Column4);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint, uint, uint> row)
        {
            return new AssemblyRefOs(image, row);
        }
    }
    
}
