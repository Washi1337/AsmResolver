using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class ClassLayoutTable : MetadataTable<MetadataRow<ushort, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ClassLayout; }
        }

        public override uint ElementByteCount
        {
            get { return sizeof(ushort) +
                         sizeof(uint) +
                         (uint)TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize;
            }
        }

        protected override MetadataRow<ushort, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<ushort, uint, uint>(token)
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadUInt32(),
                Column3 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<ushort, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16(row.Column1);
            writer.WriteUInt32(row.Column2);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.TypeDef).IndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<ushort, uint, uint> row)
        {
            return new ClassLayout(image, row);
        }
    }
}
