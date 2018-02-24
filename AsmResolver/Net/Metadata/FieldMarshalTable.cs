using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class FieldMarshalTable : SortedMetadataTable<MetadataRow<uint, uint>>
    {
        public FieldMarshalTable()
            : base(0)
        {
        }
        
        public override MetadataTokenType TokenType
        {
            get{return MetadataTokenType.FieldMarshal; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return (uint)TableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).IndexSize +
                       (uint)TableStream.BlobIndexSize;
            }
        }

        public MetadataRow<uint, uint> FindFieldMarshalOfOwner(MetadataToken ownersToken)
        {
            var encoder = TableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal);
            return (MetadataRow<uint, uint>) GetRowByKey(0, encoder.EncodeToken(ownersToken));
        }

        protected override MetadataRow<uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint>(token)
            {
                Column1 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).IndexSize),
                Column2 = reader.ReadIndex(TableStream.BlobIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column2);
        }
        
        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint> row)
        {
            return new FieldMarshal(image, row);
        }
    }
}
