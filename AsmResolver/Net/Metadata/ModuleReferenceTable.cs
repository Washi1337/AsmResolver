using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class ModuleReferenceTable : MetadataTable<MetadataRow<uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ModuleRef; }
        }

        public override uint ElementByteCount
        {
            get { return (uint) TableStream.StringIndexSize; }
        }

        protected override MetadataRow<uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            return new MetadataRow<uint>(token)
            {
                Column1 = context.Reader.ReadIndex(TableStream.StringIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint> row)
        {
            context.Writer.WriteIndex(TableStream.StringIndexSize, row.Column1);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint> row)
        {
            return new ModuleReference(image, row);
        }
    }
}
