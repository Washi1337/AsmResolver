using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class MethodPtrTable : MetadataTable<MethodPtr>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.FieldPtr; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<MethodDefinition>().IndexSize;
        }

        protected override MethodPtr ReadMember(MetadataToken token, ReadingContext context)
        {
            return new MethodPtr(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadIndex(TableStream.GetTable<MethodDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, MethodPtr member)
        {
            member.MetadataRow.Column1 = member.Method.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, MethodPtr member)
        {
            context.Writer.WriteIndex(TableStream.GetTable<MethodDefinition>().IndexSize, member.MetadataRow.Column1);
        }
    }

    public class MethodPtr : MetadataMember<MetadataRow<uint>>
    {
        private readonly LazyValue<MethodDefinition> _method;

        public MethodPtr(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            _method = new LazyValue<MethodDefinition>(() => header.GetStream<TableStream>().GetTable<MethodDefinition>()[(int)(row.Column1 -1)]);
        }

        public MethodDefinition Method
        {
            get { return _method.Value; }
            set { _method.Value = value; }
        }
    }
}
