using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class TypeReferenceTable : MetadataTable<MetadataRow<uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.TypeRef; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return (uint) TableStream.GetIndexEncoder(CodedIndex.ResolutionScope).IndexSize + // ResolutionScope
                       (uint) TableStream.StringIndexSize + // Name
                       (uint) TableStream.StringIndexSize; // Namespace
            }
        }

        protected override MetadataRow<uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint, uint>(token)
            {
                Column1 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.ResolutionScope).IndexSize),  // ResolutionScope
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),                                       // Name
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),                                       // Namespace
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.ResolutionScope).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint, uint> row)
        {
            return new TypeReference(image, row);
        }
    }
}
