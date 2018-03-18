using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class TypeDefinitionTable : MetadataTable<MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.TypeDef; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(TypeAttributes) + // Attributes
                       (uint) TableStream.StringIndexSize + // Name
                       (uint) TableStream.StringIndexSize + // Namespace
                       (uint) TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize + // BaseType
                       (uint) TableStream.GetTable(MetadataTokenType.Field).IndexSize + // FieldList
                       (uint) TableStream.GetTable(MetadataTokenType.Method).IndexSize; // MethodList  
            }
        }

        protected override MetadataRow<TypeAttributes, uint, uint, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>(token)
            {
                Column1 = (TypeAttributes) reader.ReadUInt32(), // Attributes
                Column2 = reader.ReadIndex(TableStream.StringIndexSize), // Name
                Column3 = reader.ReadIndex(TableStream.StringIndexSize), // Namespace
                Column4 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize), // BaseType
                Column5 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Field).IndexSize), // FieldList
                Column6 = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Method).IndexSize), // MethodList 
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<TypeAttributes, uint, uint, uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32((uint) row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize, row.Column4);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Field).IndexSize, row.Column5);
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Method).IndexSize, row.Column6);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<TypeAttributes, uint, uint, uint, uint, uint> row)
        {
            return new TypeDefinition(image, row);
        }
    }
}
