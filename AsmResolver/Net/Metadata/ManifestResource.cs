using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class ManifestResourceTable : MetadataTable<ManifestResource>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ManifestResource; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   sizeof (uint) +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize;
        }

        protected override ManifestResource ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new ManifestResource(Header, token, new MetadataRow<uint, uint, uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadUInt32(),
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),
                Column4 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, ManifestResource member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Offset;
            row.Column2 = (uint)member.Attributes;
            row.Column3 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
            row.Column4 = member.Implementation == null ? 0 : TableStream.GetIndexEncoder(CodedIndex.Implementation)
                .EncodeToken(member.Implementation.MetadataToken);
        }

        protected override void WriteMember(WritingContext context, ManifestResource member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt32(row.Column1);
            writer.WriteUInt32(row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize, row.Column4);
        }
    }

    public class ManifestResource : MetadataMember<MetadataRow<uint, uint, uint, uint>>, IHasCustomAttribute
    {
        private readonly LazyValue<byte[]> _data;
        private readonly LazyValue<string> _name;
        private readonly LazyValue<IImplementation> _implementation;
        private CustomAttributeCollection _customAttributes;

        public ManifestResource(string name, ManifestResourceAttributes attributes, byte[] data)
            : base(null, new MetadataToken(MetadataTokenType.ManifestResource), new MetadataRow<uint, uint, uint, uint>())
        {
            Attributes = attributes;
            _name = new LazyValue<string>(name);
            _data = new LazyValue<byte[]>(data);
            _implementation = new LazyValue<IImplementation>();
        }

        internal ManifestResource(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();

            Offset = row.Column1;
            Attributes = (ManifestResourceAttributes)row.Column2;

            _name = new LazyValue<string>(() => header.GetStream<StringStream>().GetStringByOffset(row.Column3));
            _implementation = new LazyValue<IImplementation>(() =>
            {
                var implementationToken = tableStream.GetIndexEncoder(CodedIndex.Implementation)
                    .DecodeIndex(row.Column4);
                return implementationToken.Rid != 0
                    ? (IImplementation)tableStream.ResolveMember(implementationToken)
                    : null;
            });
            _data = new LazyValue<byte[]>(() => Implementation == null && Header != null
                ? Header.NetDirectory.GetResourceData(Offset)
                : null);
        }

        public uint Offset
        {
            get;
            set;
        }

        public ManifestResourceAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public IImplementation Implementation
        {
            get { return _implementation.Value; }
            set { _implementation.Value = value; }
        }

        public bool IsEmbedded
        {
            get { return Implementation == null; }
        }

        public byte[] Data
        {
            get { return _data.Value; }
            set { _data.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                return _customAttributes = new CustomAttributeCollection(this);
            }
        }
    }
}
