
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class PropertyPtrTable : MetadataTable<MetadataRow<uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.PropertyPtr; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return (uint)TableStream.GetTable(MetadataTokenType.Property).IndexSize;
            }
        }

        protected override MetadataRow<uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            return new MetadataRow<uint>(token)
            {
                Column1 = context.Reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Property).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint> row)
        {
            context.Writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Property).IndexSize, row.Column1);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint> row)
        {
            throw new System.NotImplementedException();
        }
    }
    
}
