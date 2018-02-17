using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class EventDefinitionTable : MetadataTable<MetadataRow<EventAttributes, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Event; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(EventAttributes) +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize;
            }
        }

        protected override MetadataRow<EventAttributes, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<EventAttributes, uint, uint>(token)
            {
                Column1 = (EventAttributes) reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),
                Column3 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<EventAttributes, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16((ushort) row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<EventAttributes, uint, uint> row)
        {
            return new EventDefinition(image, row);
        }
    }
}
