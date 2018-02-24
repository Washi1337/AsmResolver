using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class ManifestResourceTable : MetadataTable<MetadataRow<uint, ManifestResourceAttributes, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ManifestResource; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(uint) +
                       sizeof(ManifestResourceAttributes) +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize;
            }
        }

        protected override MetadataRow<uint, ManifestResourceAttributes, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, ManifestResourceAttributes, uint, uint>(token)
            {
                Column1 = reader.ReadUInt32(),
                Column2 = (ManifestResourceAttributes) reader.ReadUInt32(),
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),
                Column4 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, ManifestResourceAttributes, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32(row.Column1);
            writer.WriteUInt32((uint) row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize, row.Column4);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, ManifestResourceAttributes, uint, uint> row)
        {
            return new ManifestResource(image, row);
        }
    }
}
