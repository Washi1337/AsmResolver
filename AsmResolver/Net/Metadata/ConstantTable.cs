using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class ConstantTable : MetadataTable<MetadataRow<ElementType, byte, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Constant; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(ElementType) +
                       sizeof(byte) +
                       (uint)TableStream.GetIndexEncoder(CodedIndex.HasConstant).IndexSize +
                       (uint)TableStream.BlobIndexSize;
            }
        }

        protected override MetadataRow<ElementType, byte, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<ElementType, byte, uint, uint>(token)
            {
                Column1 = (ElementType) reader.ReadByte(),
                Column2 = reader.ReadByte(),
                Column3 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasConstant).IndexSize),
                Column4 = reader.ReadIndex(TableStream.BlobIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<ElementType, byte, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteByte((byte) row.Column1);
            writer.WriteByte(row.Column2);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasConstant).IndexSize, row.Column3);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column4);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<ElementType, byte, uint, uint> row)
        {
            throw new System.NotImplementedException();
        }
    }

}
