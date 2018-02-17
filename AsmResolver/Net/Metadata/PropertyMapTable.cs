using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class PropertyMapTable : MetadataTable<MetadataRow<uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.PropertyMap; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return (uint)TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize +
                       (uint)TableStream.GetTable(MetadataTokenType.Property).IndexSize;
            }
        }

        protected override MetadataRow<uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint>(token)
            {
                Column1 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize),
                Column2 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Property).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Property).IndexSize, row.Column2);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint> row)
        {
            return new PropertyMap(image, row);
        }
    }
    
}
