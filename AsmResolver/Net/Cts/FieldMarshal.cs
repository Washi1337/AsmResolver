using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class FieldMarshal : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<IHasFieldMarshal> _parent;
        private readonly LazyValue<MarshalDescriptor> _marshalDescriptor;

        public FieldMarshal(IHasFieldMarshal parent, MarshalDescriptor marshalDescriptor)
            : base(null, new MetadataToken(MetadataTokenType.FieldMarshal))
        {
            _parent = new LazyValue<IHasFieldMarshal>(parent);
            _marshalDescriptor = new LazyValue<MarshalDescriptor>(marshalDescriptor);
        }

        internal FieldMarshal(MetadataImage image, MetadataRow<uint, uint> row)
            : base(image, row.MetadataToken)
        {
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

        public IHasFieldMarshal Parent
        {
            get { return _parent.Value; }
            set { _parent.Value = value; }
        }

        public MarshalDescriptor MarshalDescriptor
        {
            get { return _marshalDescriptor.Value; }
            set { _marshalDescriptor.Value = value; }
        }
    }
}