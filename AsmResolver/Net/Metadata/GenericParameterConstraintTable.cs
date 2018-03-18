using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class GenericParameterConstraintTable : SortedMetadataTable<MetadataRow<uint, uint>>
    {
        public GenericParameterConstraintTable()
            : base(0)
        {
        }
        
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.GenericParamConstraint; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return (uint) TableStream.GetTable(MetadataTokenType.GenericParam).IndexSize +
                       (uint) TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize;
            }
        }

        protected override MetadataRow<uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint>(token)
            {
                Column1 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.GenericParam).IndexSize),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.GenericParam).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize, row.Column2);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint> row)
        {
            return new GenericParameterConstraint(image, row);
        }
    }
}