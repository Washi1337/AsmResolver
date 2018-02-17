using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class PropertyDefinitionTable : MetadataTable<MetadataRow<PropertyAttributes, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Property; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(PropertyAttributes) +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.BlobIndexSize;
            }
        }

        protected override MetadataRow<PropertyAttributes, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<PropertyAttributes, uint, uint>(token)
            {
                Column1 = (PropertyAttributes) reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<PropertyAttributes, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16((ushort) row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<PropertyAttributes, uint, uint> row)
        {
            return new PropertyDefinition(image, row);
        }
    }
}