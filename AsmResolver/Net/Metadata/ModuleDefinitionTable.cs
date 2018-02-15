using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class ModuleDefinitionTable : MetadataTable<MetadataRow<ushort, uint, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Module; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(ushort) + // Generation
                       (uint) TableStream.StringIndexSize + // Name
                       (uint) TableStream.GuidIndexSize + // Mvid
                       (uint) TableStream.GuidIndexSize + // EncId
                       (uint) TableStream.GuidIndexSize; // EncBaseId
            }
        }

        protected override MetadataRow<ushort, uint, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<ushort, uint, uint, uint, uint>(token)
            {
                Column1 = reader.ReadUInt16(),                           // Generation
                Column2 = reader.ReadIndex(TableStream.StringIndexSize), // Name
                Column3 = reader.ReadIndex(TableStream.GuidIndexSize),   // Mvid
                Column4 = reader.ReadIndex(TableStream.GuidIndexSize),   // EncId
                Column5 = reader.ReadIndex(TableStream.GuidIndexSize)    // EncBaseId
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<ushort, uint, uint, uint, uint> row)
        {
            var writer = context.Writer;

            writer.WriteUInt16(row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.GuidIndexSize, row.Column3);
            writer.WriteIndex(TableStream.GuidIndexSize, row.Column4);
            writer.WriteIndex(TableStream.GuidIndexSize, row.Column5);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<ushort, uint, uint, uint, uint> row)
        {
            return new ModuleDefinition(image, row);
        }
    }
}
