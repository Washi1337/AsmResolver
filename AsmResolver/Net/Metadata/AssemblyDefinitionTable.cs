using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyDefinitionTable : MetadataTable<MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Assembly; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(AssemblyHashAlgorithm) +
                       sizeof(ushort) +
                       sizeof(ushort) +
                       sizeof(ushort) +
                       sizeof(ushort) +
                       sizeof(AssemblyAttributes) +
                       (uint)TableStream.BlobIndexSize +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.StringIndexSize;
            }
        }

        protected override MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>(token)
            {
                Column1 = (AssemblyHashAlgorithm) reader.ReadUInt32(),
                Column2 = reader.ReadUInt16(),
                Column3 = reader.ReadUInt16(),
                Column4 = reader.ReadUInt16(),
                Column5 = reader.ReadUInt16(),
                Column6 = (AssemblyAttributes) reader.ReadUInt32(),
                Column7 = reader.ReadIndex(TableStream.BlobIndexSize),
                Column8 = reader.ReadIndex(TableStream.StringIndexSize),
                Column9 = reader.ReadIndex(TableStream.StringIndexSize),
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32((uint) row.Column1);
            writer.WriteUInt16(row.Column2);
            writer.WriteUInt16(row.Column3);
            writer.WriteUInt16(row.Column4);
            writer.WriteUInt16(row.Column5);
            writer.WriteUInt32((uint) row.Column6);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column7);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column8);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column9);
        }

        protected override IMetadataMember CreateMemberFromRow(
            MetadataImage image, 
            MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint> row)
        {
            return new AssemblyDefinition(image, row);
        }
    }
}
