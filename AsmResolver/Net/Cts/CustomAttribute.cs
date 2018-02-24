using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class CustomAttribute : MetadataMember<MetadataRow<uint, uint, uint>>
    {
        private readonly LazyValue<IHasCustomAttribute> _parent;
        private readonly LazyValue<ICustomAttributeType> _constructor;
        private readonly LazyValue<CustomAttributeSignature> _signature;

        public CustomAttribute(ICustomAttributeType constructor, CustomAttributeSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.CustomAttribute))
        {
            _parent = new LazyValue<IHasCustomAttribute>();
            _constructor = new LazyValue<ICustomAttributeType>(constructor);
            _signature = new LazyValue<CustomAttributeSignature>(signature);
        }

        internal CustomAttribute(MetadataImage image, MetadataRow<uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var tableStream = image.Header.GetStream<TableStream>();

            _parent = new LazyValue<IHasCustomAttribute>(() =>
            {
                var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).DecodeIndex(row.Column1);
                return parentToken.Rid != 0 ? (IHasCustomAttribute)image.ResolveMember(parentToken) : null;
            });

            _constructor = new LazyValue<ICustomAttributeType>(() =>
            {
                var ctorToken = tableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).DecodeIndex(row.Column2);
                return ctorToken.Rid != 0 ? (ICustomAttributeType)image.ResolveMember(ctorToken) : null;
            });

            _signature = new LazyValue<CustomAttributeSignature>(() => CustomAttributeSignature.FromReader(this,
                tableStream.MetadataHeader.GetStream<BlobStream>().CreateBlobReader(row.Column3)));
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

        public override string ToString()
        {
            return Constructor.ToString();
        }

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            var tableStream = buffer.TableStreamBuffer;
            tableStream.GetTable<CustomAttributeTable>().Add(new MetadataRow<uint, uint, uint>
            {
                Column1 = tableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).EncodeToken(Parent.MetadataToken),
                Column2 = tableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).EncodeToken(Constructor.MetadataToken),
                Column3 = buffer.BlobStreamBuffer.GetBlobOffset(Signature)
            });
        }
    }
}
