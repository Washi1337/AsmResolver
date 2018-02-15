using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class MethodPtrTable : MetadataTable<MetadataRow<uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.FieldPtr; }
        }

        public override uint ElementByteCount
        {
            get { return (uint) TableStream.GetTable(MetadataTokenType.Method).IndexSize; }
        }

        protected override MetadataRow<uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            return new MetadataRow<uint>(token)
            {
                Column1 = context.Reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Method).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint> row)
        {
            context.Writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Method).IndexSize, row.Column1);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint> row)
        {
            throw new System.NotImplementedException();
        }
    }
}
