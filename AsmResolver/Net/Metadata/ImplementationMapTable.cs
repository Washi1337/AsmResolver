using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class ImplementationMapTable : MetadataTable<MetadataRow<ImplementationMapAttributes, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ImplMap; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(ImplementationMapAttributes) +
                       (uint) TableStream.GetIndexEncoder(CodedIndex.MemberForwarded).IndexSize +
                       (uint) TableStream.StringIndexSize +
                       (uint) TableStream.GetTable(MetadataTokenType.ModuleRef).IndexSize;
            }
        }

        public MetadataRow<ImplementationMapAttributes, uint, uint, uint> FindImplementationMapOfOwner(MetadataToken ownersToken)
        {
            var encodedIndex = TableStream.GetIndexEncoder(CodedIndex.MemberForwarded).EncodeToken(ownersToken);
            return (MetadataRow<ImplementationMapAttributes, uint, uint, uint>) GetRowByKey(1, encodedIndex);
        }

        protected override MetadataRow<ImplementationMapAttributes, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<ImplementationMapAttributes, uint, uint, uint>(token)
            {
                Column1 = (ImplementationMapAttributes) reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.MemberForwarded).IndexSize),
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),
                Column4 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.ModuleRef).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<ImplementationMapAttributes, uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16((ushort) row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.MemberForwarded).IndexSize, row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.ModuleRef).IndexSize, row.Column4);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<ImplementationMapAttributes, uint, uint, uint> row)
        {
            return new ImplementationMap(image, row);
        }
    }
}
