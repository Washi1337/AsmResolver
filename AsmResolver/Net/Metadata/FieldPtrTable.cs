using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class FieldPtrTable : MetadataTable<MetadataRow<uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.FieldPtr; }
        }

        public override uint ElementByteCount
        {
            get { return (uint) TableStream.GetTable(MetadataTokenType.Field).IndexSize; }
        }

        protected override MetadataRow<uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            return new MetadataRow<uint>(token)
            {
                Column1 = context.Reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Field).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint> row)
        {
            context.Writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Field).IndexSize, row.Column1);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint> row)
        {
            throw new System.NotImplementedException();
        }
    }
}
