using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class EncLogTable : MetadataTable<MetadataRow<uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.EncLog; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(uint) +
                       sizeof(uint);
            }
        }

        protected override MetadataRow<uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint>(token)
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadUInt32(),
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32(row.Column1);
            writer.WriteUInt32(row.Column2);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint> row)
        {
            throw new System.NotImplementedException();
        }
    }
}
