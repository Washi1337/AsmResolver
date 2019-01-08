using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents extra metadata associated to a member that has a custom marshal descriptor defined.
    /// </summary>
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
                return tableStream.TryResolveRow(parentToken, out var parentRow)
                    ? (IHasFieldMarshal) tableStream.GetTable(parentToken.TokenType).GetMemberFromRow(image, parentRow)
                    : null;
            });

            _marshalDescriptor = new LazyValue<MarshalDescriptor>(() => 
                MarshalDescriptor.FromReader(image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column2), true));
        }

        /// <inheritdoc />
        public override MetadataImage Image => _parent.IsInitialized && _parent.Value != null 
            ? _parent.Value.Image 
            : _image;

        /// <summary>
        /// Gets the member associated to this extra metadata.
        /// </summary>
        public IHasFieldMarshal Parent
        {
            get => _parent.Value;
            internal set
            {
                _parent.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets or sets the custom marshal descriptor that is assigned to the member.
        /// </summary>
        public MarshalDescriptor MarshalDescriptor
        {
            get => _marshalDescriptor.Value;
            set => _marshalDescriptor.Value = value;
        }
    }
}