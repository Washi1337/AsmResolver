using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class MethodSpecificationTable : MetadataTable<MetadataRow<uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.MethodSpec; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return (uint) TableStream.GetTable(MetadataTokenType.Method).IndexSize +
                       (uint) TableStream.BlobIndexSize;
            }
        }

        protected override MetadataRow<uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint>(token)
            {
                Column1 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Method).IndexSize),
                Column2 = reader.ReadIndex(TableStream.BlobIndexSize)
            };
        }
        
        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Method).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column2);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint> row)
        {
            return new MethodSpecification(image, row);
        }
    }
    
}
