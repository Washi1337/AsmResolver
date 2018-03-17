using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class FieldMarshal : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<IHasFieldMarshal> _parent;
        private readonly LazyValue<MarshalDescriptor> _marshalDescriptor;
        private MetadataImage _image;

        public FieldMarshal(MarshalDescriptor marshalDescriptor)
            : base(new MetadataToken(MetadataTokenType.FieldMarshal))
        {
            _parent = new LazyValue<IHasFieldMarshal>();
            _marshalDescriptor = new LazyValue<MarshalDescriptor>(marshalDescriptor);
        }

        internal FieldMarshal(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            _parent = new LazyValue<IHasFieldMarshal>(() =>
            {
                var tableStream = image.Header.GetStream<TableStream>();
                var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).DecodeIndex(row.Column1);
                MetadataRow parentRow;
                return tableStream.TryResolveRow(parentToken, out parentRow)
                    ? (IHasFieldMarshal) tableStream.GetTable(parentToken.TokenType).GetMemberFromRow(image, parentRow)
                    : null;
            });

            _marshalDescriptor = new LazyValue<MarshalDescriptor>(() => 
                MarshalDescriptor.FromReader(image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column2)));
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _parent.IsInitialized && _parent.Value != null ? _parent.Value.Image : _image; }
        }

        public IHasFieldMarshal Parent
        {
            get { return _parent.Value; }
            internal set
            {
                _parent.Value = value;
                _image = null;
            }
        }

        public MarshalDescriptor MarshalDescriptor
        {
            get { return _marshalDescriptor.Value; }
            set { _marshalDescriptor.Value = value; }
        }
    }
}