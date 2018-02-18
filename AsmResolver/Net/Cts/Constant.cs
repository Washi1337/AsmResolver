using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class Constant : MetadataMember<MetadataRow<ElementType, byte, uint, uint>>
    {
        private readonly LazyValue<IHasConstant> _parent;
        private readonly LazyValue<DataBlobSignature> _value;

        public Constant(ElementType constantType, DataBlobSignature value)
            : base(null, new MetadataToken(MetadataTokenType.Constant))
        {
            ConstantType = constantType;
            _value = new LazyValue<DataBlobSignature>(value);
        }

        internal Constant(MetadataImage image, MetadataRow<ElementType, byte, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            ConstantType = row.Column1;

            _parent = new LazyValue<IHasConstant>(() =>
            {
                var tableStream = image.Header.GetStream<TableStream>();
                var hasConstantToken = tableStream.GetIndexEncoder(CodedIndex.HasConstant).DecodeIndex(row.Column3);
                return hasConstantToken.Rid != 0 ? (IHasConstant) image.ResolveMember(hasConstantToken) : null;
            });

            _value = new LazyValue<DataBlobSignature>(() => 
                DataBlobSignature.FromReader(image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column4)));
        }

        public ElementType ConstantType
        {
            get;
            set;
        }

        public IHasConstant Parent
        {
            get { return _parent.Value; }
            set { _parent.Value = value; }
        }

        public DataBlobSignature Value
        {
            get { return _value.Value; }
            set { _value.Value = value; }
        }
    }
}
