using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class NestedClassTable : MetadataTable<MetadataRow<uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.NestedClass; }
        }

        public override uint ElementByteCount
        {
            get
            {
                var definitionTable = TableStream.GetTable(MetadataTokenType.TypeDef);
                return (uint) definitionTable.IndexSize +
                       (uint) definitionTable.IndexSize;
            }
        }

        protected override MetadataRow<uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            var definitionTable = TableStream.GetTable(MetadataTokenType.TypeDef);
            return new MetadataRow<uint, uint>(token)
            {
                Column1 = reader.ReadIndex(definitionTable.IndexSize),
                Column2 = reader.ReadIndex(definitionTable.IndexSize),
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint> row)
        {
            var writer = context.Writer;
            var definitionTable = TableStream.GetTable(MetadataTokenType.TypeDef);

            writer.WriteIndex(definitionTable.IndexSize, row.Column1);
            writer.WriteIndex(definitionTable.IndexSize, row.Column2);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint> row)
        {
            return new NestedClass(image, row);
        }
    }

}
