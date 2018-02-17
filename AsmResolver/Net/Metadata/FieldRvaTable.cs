using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class FieldRvaTable : MetadataTable<MetadataRow<uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.FieldRva; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(uint) +
                       (uint)TableStream.GetTable(MetadataTokenType.Field).IndexSize;
            }
        }

        protected override MetadataRow<uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint>(token)
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Field).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32(row.Column1);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Field).IndexSize, row.Column2);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint> row)
        {
            return new FieldRva(image, row);
        }
    }
}
