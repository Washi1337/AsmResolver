using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class MemberReferenceTable : MetadataTable<MetadataRow<uint, uint, uint>> {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.MemberRef; }
        }

        public override uint ElementByteCount
        {
            get {
                return (uint)TableStream.GetIndexEncoder(CodedIndex.MemberRefParent).IndexSize +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.BlobIndexSize;
            }
        }

        protected override MetadataRow<uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint, uint>(token)
            {
                Column1 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.MemberRefParent).IndexSize),
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.MemberRefParent).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint, uint> row)
        {
            return new MemberReference(image, row);
        }
    }
    
}
