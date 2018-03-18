
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class StandAloneSignatureTable : MetadataTable<MetadataRow<uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.StandAloneSig; }
        }

        public override uint ElementByteCount
        {
            get { return (uint) TableStream.BlobIndexSize; }
        }

        protected override MetadataRow<uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            return new MetadataRow<uint>(token)
            {
                Column1 = context.Reader.ReadIndex(TableStream.BlobIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint> row)
        {
            context.Writer.WriteIndex(TableStream.BlobIndexSize, row.Column1);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint> row)
        {
            return new StandAloneSignature(image, row);
        }
    }
}
