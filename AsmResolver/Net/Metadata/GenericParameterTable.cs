using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class GenericParameterTable : MetadataTable<MetadataRow<ushort, GenericParameterAttributes, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.GenericParam; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(ushort) +
                       sizeof(GenericParameterAttributes) +
                       (uint)TableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef).IndexSize +
                       (uint)TableStream.StringIndexSize;
            }
        }

        protected override MetadataRow<ushort, GenericParameterAttributes, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<ushort, GenericParameterAttributes, uint, uint>(token)
            {
                Column1 = reader.ReadUInt16(),
                Column2 = (GenericParameterAttributes) reader.ReadUInt16(),
                Column3 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef).IndexSize),
                Column4 = reader.ReadIndex(TableStream.StringIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<ushort, GenericParameterAttributes, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16(row.Column1);
            writer.WriteUInt16((ushort) row.Column2);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef).IndexSize, row.Column3);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column4);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<ushort, GenericParameterAttributes, uint, uint> row)
        {
            return new GenericParameter(image, row);
        }
    }
    
}
