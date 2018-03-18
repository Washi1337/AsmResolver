using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Metadata
{
    public class FileReferenceTable : MetadataTable<MetadataRow<FileAttributes, uint, uint>>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.File; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(FileAttributes) +
                       (uint)TableStream.StringIndexSize +
                       (uint)TableStream.BlobIndexSize;
            }
        }

        protected override MetadataRow<FileAttributes, uint, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            return new MetadataRow<FileAttributes, uint, uint>(token)
            {
                Column1 = (FileAttributes) reader.ReadUInt32(),
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize)
            };
        }

        protected override void WriteRow(WritingContext context, MetadataRow<FileAttributes, uint, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32((uint) row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<FileAttributes, uint, uint> row)
        {
            return new FileReference(image, row);
        }
    }
    
}
