using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

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
            // TODO: native type
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
        internal FieldMarshal(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();
            
            var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).DecodeIndex(row.Column1);
            if (parentToken.Rid != 0)
                Parent = (IHasFieldMarshal)tableStream.ResolveMember(parentToken);

            // TODO: native type
        }

        public IHasFieldMarshal Parent
        {
            get;
            set;
        }
    }
}
