using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class ParamPtrTable : MetadataTable<ParamPtr>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ParamPtr; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<ParameterDefinition>().IndexSize;
        }

        protected override ParamPtr ReadMember(MetadataToken token, ReadingContext context)
        {
            return new ParamPtr(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadIndex(TableStream.GetTable<ParameterDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, ParamPtr member)
        {
            member.MetadataRow.Column1 = member.Parameter.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, ParamPtr member)
        {
            context.Writer.WriteIndex(TableStream.GetTable<ParameterDefinition>().IndexSize, member.MetadataRow.Column1);
        }
    }

    public class ParamPtr : MetadataMember<MetadataRow<uint>>
    {
        public ParamPtr(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            Parameter = header.GetStream<TableStream>().GetTable<ParameterDefinition>()[(int)(row.Column1 - 1)];
        }

        public ParameterDefinition Parameter
        {
            get;
            set;
        }
    }
}
