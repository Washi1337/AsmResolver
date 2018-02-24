using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class CustomAttributeTable : SortedMetadataTable<MetadataRow<uint, uint, uint>>
    {
        public CustomAttributeTable()
            : base(0)
        {
        }
        
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.CustomAttribute; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return (uint)TableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).IndexSize +
                       (uint)TableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).IndexSize +
                       (uint)TableStream.BlobIndexSize;
            }
        }

        protected override MetadataRow<uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint, uint>(token)
            {
                Column1 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).IndexSize),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).IndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize),
            };
        }
        
        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).IndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint, uint> row)
        {
            return new CustomAttribute(image, row);
        }
    }
    
}
