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
        private readonly LazyValue<IHasCustomAttribute> _parent;
        private readonly LazyValue<ICustomAttributeType> _constructor;
        private readonly LazyValue<CustomAttributeSignature> _signature;

        public CustomAttribute(ICustomAttributeType constructor, CustomAttributeSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.CustomAttribute), new MetadataRow<uint, uint, uint>())
        {
            _parent = new LazyValue<IHasCustomAttribute>();
            _constructor = new LazyValue<ICustomAttributeType>(constructor);
            _signature = new LazyValue<CustomAttributeSignature>(signature);
        }

        internal CustomAttribute(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();

            _parent = new LazyValue<IHasCustomAttribute>(() =>
            {
                var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).DecodeIndex(row.Column1);
                return parentToken.Rid != 0 ? (IHasCustomAttribute)tableStream.ResolveMember(parentToken) : null;
            });

            _constructor = new LazyValue<ICustomAttributeType>(() =>
            {
                var ctorToken = tableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).DecodeIndex(row.Column2);
                return ctorToken.Rid != 0 ? (ICustomAttributeType)tableStream.ResolveMember(ctorToken) : null;
            });

            _signature = new LazyValue<CustomAttributeSignature>(() => CustomAttributeSignature.FromReader(this,
                Header.GetStream<BlobStream>().CreateBlobReader(MetadataRow.Column3)));
        }

        public IHasCustomAttribute Parent
        {
            get { return _parent.Value; }
            internal set { _parent.Value = value; }
        }

        public ICustomAttributeType Constructor
        {
            get { return _constructor.Value; }
            set { _constructor.Value = value; }
        }

        public CustomAttributeSignature Signature
        {
            get { return _signature.Value; }
            set { _signature.Value = value; }
        }
    }
}
