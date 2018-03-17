using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a custom attribute that can be assigned to a member to provide additional information about the member.
    /// </summary>
    public class CustomAttribute : MetadataMember<MetadataRow<uint, uint, uint>>
    {
        private readonly LazyValue<IHasCustomAttribute> _parent;
        private readonly LazyValue<ICustomAttributeType> _constructor;
        private readonly LazyValue<CustomAttributeSignature> _signature;
        private MetadataImage _image;

        public CustomAttribute(ICustomAttributeType constructor, CustomAttributeSignature signature)
            : base(new MetadataToken(MetadataTokenType.CustomAttribute))
        {
            _parent = new LazyValue<IHasCustomAttribute>();
            _constructor = new LazyValue<ICustomAttributeType>(constructor);
            _signature = new LazyValue<CustomAttributeSignature>(signature);
        }

        internal CustomAttribute(MetadataImage image, MetadataRow<uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            var tableStream = image.Header.GetStream<TableStream>();

            _parent = new LazyValue<IHasCustomAttribute>(() =>
            {
                var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).DecodeIndex(row.Column1);
                IMetadataMember member;
                return image.TryResolveMember(parentToken, out member)
                    ? (IHasCustomAttribute) member
                    : null;
            });

            _constructor = new LazyValue<ICustomAttributeType>(() =>
            {
                var ctorToken = tableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).DecodeIndex(row.Column2);
                IMetadataMember member;
                return image.TryResolveMember(ctorToken, out member)
                    ? (ICustomAttributeType) member
                    : null;
            });

            _signature = new LazyValue<CustomAttributeSignature>(() => CustomAttributeSignature.FromReader(this,
                tableStream.MetadataHeader.GetStream<BlobStream>().CreateBlobReader(row.Column3)));
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _parent.IsInitialized && _parent.Value != null ? _parent.Value.Image : _image; }
        }

        /// <summary>
        /// Gets the member that the custom attribute is assigned to.
        /// </summary>
        public IHasCustomAttribute Parent
        {
            get { return _parent.Value; }
            internal set
            {
                _parent.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the constructor that is used to initialize the attribute.
        /// </summary>
        public ICustomAttributeType Constructor
        {
            get { return _constructor.Value; }
            set { _constructor.Value = value; }
        }

        /// <summary>
        /// Gets or sets the signature containing the arguments used to invoke the constructor and initialize the attribute. 
        /// </summary>
        public CustomAttributeSignature Signature
        {
            get { return _signature.Value; }
            set { _signature.Value = value; }
        }

        public override string ToString()
        {
            return Constructor.ToString();
        }
    }
}
