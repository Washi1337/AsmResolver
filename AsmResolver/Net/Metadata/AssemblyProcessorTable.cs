using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyProcessorTable : MetadataTable<MetadataRow<uint>> 
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyProcessor; }
        }

        public override uint ElementByteCount
        {
            get { return sizeof(uint); }
        }
        
        protected override MetadataRow<uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            return new MetadataRow<uint>(token)
            {
                Column1 = context.Reader.ReadUInt32(),
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint> row)
        {
            context.Writer.WriteUInt32(row.Column1);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint> row)
        {
            return new AssemblyProcessor(image, row);
        }
    }
}
