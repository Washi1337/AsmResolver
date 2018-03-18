using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyReferenceTable : MetadataTable<MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyRef; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(ushort) +
                       sizeof(ushort) +
                       sizeof(ushort) +
                       sizeof(ushort) +
                       sizeof(AssemblyAttributes) +
                       (uint)TableStream.BlobIndexSize +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.BlobIndexSize;
            }
        }

        protected override MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>(token)
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadUInt16(),
                Column3 = reader.ReadUInt16(),
                Column4 = reader.ReadUInt16(),
                Column5 = (AssemblyAttributes) reader.ReadUInt32(),
                Column6 = reader.ReadIndex(TableStream.BlobIndexSize),
                Column7 = reader.ReadIndex(TableStream.StringIndexSize),
                Column8 = reader.ReadIndex(TableStream.StringIndexSize),
                Column9 = reader.ReadIndex(TableStream.BlobIndexSize),
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt16(row.Column1);
            writer.WriteUInt16(row.Column2);
            writer.WriteUInt16(row.Column3);
            writer.WriteUInt16(row.Column4);
            writer.WriteUInt32((uint) row.Column5);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column6);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column7);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column8);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column9);
        }

        protected override IMetadataMember CreateMemberFromRow(
            MetadataImage image, 
            MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint> row)
        {
            return new AssemblyReference(image, row);
        }
    }
    
}
