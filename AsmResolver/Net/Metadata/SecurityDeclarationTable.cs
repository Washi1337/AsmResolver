using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class SecurityDeclarationTable : MetadataTable<MetadataRow<SecurityAction, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.DeclSecurity; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(ushort) +
                       (uint)TableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).IndexSize +
                       (uint)TableStream.BlobIndexSize;
            }
        }

        protected override MetadataRow<SecurityAction, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<SecurityAction, uint, uint>(token)
            {
                Column1 = (SecurityAction) reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).IndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize),
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<SecurityAction, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16((ushort) row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).IndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<SecurityAction, uint, uint> row)
        {
            return new SecurityDeclaration(image, row);
        }
    }
    
}
