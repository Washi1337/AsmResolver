using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class ParameterDefinitionTable : MetadataTable<MetadataRow<ParameterAttributes, ushort, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Param; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(ParameterAttributes) + // Attributes
                       sizeof(ushort) + // Sequence
                       (uint) TableStream.StringIndexSize; // Name
            }
        }

        protected override MetadataRow<ParameterAttributes, ushort, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<ParameterAttributes, ushort, uint>(token)
            {
                Column1 = (ParameterAttributes) reader.ReadUInt16(),
                Column2 = reader.ReadUInt16(),
                Column3 = reader.ReadIndex(TableStream.StringIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<ParameterAttributes, ushort, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16((ushort) row.Column1);
            writer.WriteUInt16(row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<ParameterAttributes, ushort, uint> row)
        {
            return new ParameterDefinition(image, row);
        }
    }
}