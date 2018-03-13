using AsmResolver.Net.Cts;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class FieldRvaTable : SortedMetadataTable<MetadataRow<DataSegment, uint>>
    {
        public FieldRvaTable()
            : base(1)
        {
        }

        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.FieldRva; }
        }

        public override uint ElementByteCount
        {
            get
            {
                return sizeof(uint) +
                       (uint)TableStream.GetTable(MetadataTokenType.Field).IndexSize;
            }
        }

        public MetadataRow<DataSegment, uint> FindFieldRvaOfField(uint fieldRid)
        {
            return GetRowByKey(1, fieldRid);
        }

        protected override MetadataRow<DataSegment, uint> ReadRow(ReadingContext context, MetadataToken token)
        {
            var reader = context.Reader;
            
            uint rva = reader.ReadUInt32();
            uint fieldRid = reader.ReadIndex(TableStream.GetTable(MetadataTokenType.Field).IndexSize);

            DataSegment dataSegment = null;
            if (rva != 0)
            {
                int dataSize = GetDataSize(fieldRid);
                var dataReader = context.Reader.CreateSubReader(context.Assembly.RvaToFileOffset(rva), dataSize);
                dataSegment = DataSegment.FromReader(dataReader);
            }
            
            return new MetadataRow<DataSegment, uint>(token)
            {
                Column1 = dataSegment,
                Column2 = fieldRid
            };
        }
        
        private int GetDataSize(uint fieldRid)
        {
            var fieldTable = (FieldDefinitionTable) TableStream.GetTable(MetadataTokenType.Field);
            var blobReader = TableStream.MetadataHeader.GetStream<BlobStream>().CreateBlobReader(fieldTable[(int) (fieldRid - 1)].Column3);

            blobReader.ReadByte();
            var elementType = (ElementType) blobReader.ReadByte();
            
            switch (elementType)
            {
                case ElementType.Boolean:
                case ElementType.I1:
                case ElementType.U1:
                    return sizeof (byte);
                case ElementType.I2:
                case ElementType.U2:
                    return sizeof (ushort);
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.R4:
                    return sizeof (uint);
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R8:
                    return sizeof (ulong);
                case ElementType.Class:
                case ElementType.ValueType:
                    uint codedIndex;
                    if (!blobReader.TryReadCompressedUInt32(out codedIndex))
                        return 0;
                    var typeToken = TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(codedIndex);
                    if (typeToken.TokenType == MetadataTokenType.TypeDef)
                    {
                        var classLayoutTable = (ClassLayoutTable) TableStream.GetTable(MetadataTokenType.ClassLayout);
                        var row = classLayoutTable.GetRowByKey(2, typeToken.Rid);
                        if (row != null)
                            return (int) row.Column2;
                    }

                    return 0;
                case ElementType.I:
                case ElementType.U:
                // TODO;
                default:
                    return 0;
            }
            
        }
        
        protected override void WriteRow(WritingContext context, MetadataRow<DataSegment, uint> row)
        {
            var writer = context.Writer;
            writer.WriteUInt32((uint) row.Column1.GetRva(context.Assembly));
            writer.WriteIndex(TableStream.GetTable(MetadataTokenType.Field).IndexSize, row.Column2);
        }

        protected override IMetadataMember CreateMemberFromRow(MetadataImage image, MetadataRow<DataSegment, uint> row)
        {
            return new FieldRva(image, row);
        }
    }
}
