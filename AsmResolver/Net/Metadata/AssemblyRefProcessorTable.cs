using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyRefProcessorTable : MetadataTable<MetadataRow<uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyRefProcessor; }
        }

        public override uint ElementByteCount
        {
            get { return sizeof(uint) +
                         (uint)TableStream.GetTable(MetadataTokenType.AssemblyRef).IndexSize;
            }
        }

        protected override MetadataRow<uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, uint>(token)
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.AssemblyRef).IndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32(row.Column1);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.AssemblyRef).IndexSize, row.Column2);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, uint> row)
        {
            return new AssemblyRefProcessor(image, row);
        }
    }
    
}
