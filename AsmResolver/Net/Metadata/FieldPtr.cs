using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class FieldPtrTable : MetadataTable<FieldPtr>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.FieldPtr; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<FieldDefinition>().IndexSize;
        }

        protected override FieldPtr ReadMember(MetadataToken token, ReadingContext context)
        {
            return new FieldPtr(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadIndex(TableStream.GetTable<FieldDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, FieldPtr member)
        {
            member.MetadataRow.Column1 = member.Field.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, FieldPtr member)
        {
            context.Writer.WriteIndex(TableStream.GetTable<FieldDefinition>().IndexSize, member.MetadataRow.Column1);
        }
    }

    public class FieldPtr : MetadataMember<MetadataRow<uint>>
    {
        public FieldPtr(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            Field = header.GetStream<TableStream>().GetTable<FieldDefinition>()[(int)(row.Column1 - 1)];
        }

        public FieldDefinition Field
        {
            get;
            set;
        }
    }
}
