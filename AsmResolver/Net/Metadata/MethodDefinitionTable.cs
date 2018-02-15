using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class MethodDefinitionTable : MetadataTable<MetadataRow<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Method; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(uint) + // Rva
                       sizeof(ushort) + // ImplAttrbibutes
                       sizeof(ushort) + // Attributes
                       (uint) TableStream.StringIndexSize + // Name
                       (uint) TableStream.BlobIndexSize + // Signature
                       (uint) TableStream.GetTable(MetadataTokenType.Param).IndexSize; // ParamList
            }
        }

        protected override MetadataRow<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint>(token)
            {
                Column1 = reader.ReadUInt32(),                                                      // Rva
                Column2 = (MethodImplAttributes) reader.ReadUInt16(),                               // ImplAttrbibutes
                Column3 = (MethodAttributes) reader.ReadUInt16(),                                   // Attributes
                Column4 = reader.ReadIndex(TableStream.StringIndexSize),                            // Name
                Column5 = reader.ReadIndex(TableStream.BlobIndexSize),                              // Signature
                Column6 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Param).IndexSize) // ParamList
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32(row.Column1);
            writer.WriteUInt16((ushort) row.Column2);
            writer.WriteUInt16((ushort) row.Column3);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column4);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column5);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Param).IndexSize, row.Column6);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint> row)
        {
            return new MethodDefinition(image, row);
        }
    }
}