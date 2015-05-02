using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class FieldMarshalTable : MetadataTable<FieldMarshal>
    {
        public override MetadataTokenType TokenType
        {
            get{return MetadataTokenType.FieldMarshal; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).IndexSize +
                   (uint)TableStream.BlobIndexSize;
        }

        protected override FieldMarshal ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new FieldMarshal(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).IndexSize),
                Column2 = reader.ReadIndex(TableStream.BlobIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, FieldMarshal member)
        {
            var row = member.MetadataRow;

            row.Column1 = TableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).EncodeToken(member.Parent.MetadataToken);
            row.Column2 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.MarshalDescriptor);
        }

        protected override void WriteMember(WritingContext context, FieldMarshal member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).IndexSize, row.Column1);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column2);
        }
    }
    public class FieldMarshal : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<IHasFieldMarshal> _parent;
        private readonly LazyValue<MarshalDescriptor> _marshalDescriptor;

        public FieldMarshal(IHasFieldMarshal parent, MarshalDescriptor marshalDescriptor)
            : base(null, new MetadataToken(MetadataTokenType.FieldMarshal), new MetadataRow<uint, uint>())
        {
            _parent = new LazyValue<IHasFieldMarshal>(parent);
            _marshalDescriptor = new LazyValue<MarshalDescriptor>(marshalDescriptor);
        }

        internal FieldMarshal(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();
            var blobStream = header.GetStream<BlobStream>();

            _parent = new LazyValue<IHasFieldMarshal>(() =>
            {
                var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).DecodeIndex(row.Column1);
                return parentToken.Rid != 0 ? (IHasFieldMarshal)tableStream.ResolveMember(parentToken) : null;
            });

            _marshalDescriptor = new LazyValue<MarshalDescriptor>(() => 
                MarshalDescriptor.FromReader(blobStream.CreateBlobReader(row.Column2)));
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
