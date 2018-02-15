using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class MethodSemanticsTable : MetadataTable<MetadataRow<MethodSemanticsAttributes, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.MethodSemantics; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(MethodSemanticsAttributes) +
                       (uint)TableStream.GetTable(MetadataTokenType.Method).IndexSize +
                       (uint)TableStream.GetIndexEncoder(CodedIndex.HasSemantics).IndexSize;
            }
        }

        protected override MetadataRow<MethodSemanticsAttributes, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<MethodSemanticsAttributes, uint, uint>(token)
            {
                Column1 = (MethodSemanticsAttributes) reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Method).IndexSize),
                Column3 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasSemantics).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<MethodSemanticsAttributes, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16((ushort) row.Column1);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Method).IndexSize, row.Column2);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasSemantics).IndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<MethodSemanticsAttributes, uint, uint> row)
        {
            throw new System.NotImplementedException();
        }
    }
    
}
