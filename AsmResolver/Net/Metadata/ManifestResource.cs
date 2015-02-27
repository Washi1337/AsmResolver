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
        private CustomAttributeCollection _customAttributes;
        private byte[] _data;

        public ManifestResource(string name, ManifestResourceAttributes attributes, byte[] data)
            : base(null, new MetadataToken(MetadataTokenType.ManifestResource), new MetadataRow<uint, uint, uint, uint>())
        {
            Name = name;
            Attributes = attributes;
            Data = data;
        }

        internal ManifestResource(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();

            Offset = row.Column1;
            Attributes = (ManifestResourceAttributes)row.Column2;
            Name = header.GetStream<StringStream>().GetStringByOffset(row.Column3);

            var implementationToken = tableStream.GetIndexEncoder(CodedIndex.Implementation).DecodeIndex(row.Column4);
            if (implementationToken.Rid != 0)
                Implementation = (IImplementation)tableStream.ResolveMember(implementationToken);
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
            get;
            set;
        }

        public IImplementation Implementation
        {
            get;
            set;
        }

        public bool IsEmbedded
        {
            get { return Implementation == null; }
        }

        public byte[] Data
        {
            get
            {
                if (_data != null)
                    return _data;
                if (Implementation == null && Header != null)
                    return _data = Header.NetDirectory.GetResourceData(Offset);
                return null;
            }
            set { _data = value; }
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
