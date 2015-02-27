using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class CustomAttributeTable : MetadataTable<CustomAttribute>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.CustomAttribute; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).IndexSize +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).IndexSize +
                   (uint)TableStream.BlobIndexSize;
        }

        protected override CustomAttribute ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new CustomAttribute(Header, token, new MetadataRow<uint, uint, uint>()
            {
                Column1 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).IndexSize),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).IndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, CustomAttribute member)
        {
            var row = member.MetadataRow;
            row.Column1 = TableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute)
                .EncodeToken(member.Parent.MetadataToken);
            row.Column2 = TableStream.GetIndexEncoder(CodedIndex.CustomAttributeType)
                .EncodeToken(member.Constructor.MetadataToken);
            row.Column3 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.Signature);
        }

        protected override void WriteMember(WritingContext context, CustomAttribute member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).IndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }
    }

    public class CustomAttribute : MetadataMember<MetadataRow<uint, uint, uint>>
    {
        public CustomAttribute(ICustomAttributeType constructor, CustomAttributeSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.CustomAttribute), new MetadataRow<uint, uint, uint>())
        {
            Constructor = constructor;
            Signature = signature;
        }

        internal CustomAttribute(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();

            var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).DecodeIndex(row.Column1);
            if (parentToken.Rid != 0)
                Parent = (IHasCustomAttribute)tableStream.ResolveMember(parentToken);

            var ctorToken = tableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).DecodeIndex(row.Column2);
            if (ctorToken.Rid != 0)
                Constructor = (ICustomAttributeType)tableStream.ResolveMember(ctorToken);

            Signature = CustomAttributeSignature.FromReader(this,
                Header.GetStream<BlobStream>().CreateBlobReader(MetadataRow.Column3));
        }

        public IHasCustomAttribute Parent
        {
            get;
            internal set;
        }

        public ICustomAttributeType Constructor
        {
            get;
            set;
        }

        public CustomAttributeSignature Signature
        {
            get;
            set;
        }
    }
}
