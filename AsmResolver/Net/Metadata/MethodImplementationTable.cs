using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class MethodImplementationTable : MetadataTable<MetadataRow<uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.MethodImpl; }
        }

        public override uint ElementByteCount
        {
            get
            {
                var encoder = TableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
                return (uint) TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize +
                       (uint) encoder.IndexSize +
                       (uint) encoder.IndexSize;
            }
        }

        protected override MetadataRow<uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            var encoder = TableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
            return new MetadataRow<uint, uint, uint>(token)
            {
                Column1 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize),
                Column2 = reader.ReadIndex(encoder.IndexSize),
                Column3 = reader.ReadIndex(encoder.IndexSize),
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint, uint> row)
        {
            var writer = context.Writer;
            var encoder = TableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize, row.Column1);
            writer.WriteIndex(encoder.IndexSize, row.Column2);
            writer.WriteIndex(encoder.IndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint, uint> row)
        {
            return new MethodImplementation(image, row);
        }
    }
    
}
