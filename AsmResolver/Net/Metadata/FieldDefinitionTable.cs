using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class FieldDefinitionTable : MetadataTable<MetadataRow<FieldAttributes, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Field; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(FieldAttributes) + // Attributes
                       (uint) TableStream.StringIndexSize + // Name
                       (uint) TableStream.BlobIndexSize; // Signature 
            }
        }

        protected override MetadataRow<FieldAttributes, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<FieldAttributes, uint, uint>(token)
            {
                Column1 = (FieldAttributes) reader.ReadUInt16(),         // Attributes
                Column2 = reader.ReadIndex(TableStream.StringIndexSize), // Name
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize),   // Signature
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<FieldAttributes, uint, uint> row)
        {
            var writer = context.Writer;

            writer.WriteUInt16((ushort) row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<FieldAttributes, uint, uint> row)
        {
            return new FieldDefinition(image, row);
        }
    }
    
}
