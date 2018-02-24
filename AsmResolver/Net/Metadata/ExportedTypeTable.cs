using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class ExportedTypeTable : MetadataTable<MetadataRow<TypeAttributes, uint, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ExportedType; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(uint) +
                       sizeof(uint) +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize;
            }
        }

        protected override MetadataRow<TypeAttributes, uint, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<TypeAttributes, uint, uint, uint, uint>(token)
            {
                Column1 = (TypeAttributes) reader.ReadUInt32(),
                Column2 = reader.ReadUInt32(),
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),
                Column4 = reader.ReadIndex(TableStream.StringIndexSize),
                Column5 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize),
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<TypeAttributes, uint, uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32((uint) row.Column1);
            writer.WriteUInt32(row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column4);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize, row.Column5);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<TypeAttributes, uint, uint, uint, uint> row)
        {
            return new ExportedType(image, row);
        }
    }
}
