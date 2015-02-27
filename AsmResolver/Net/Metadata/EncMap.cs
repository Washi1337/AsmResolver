using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class EncMapTable : MetadataTable<EncMap>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.EncMap; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint);
        }

        protected override EncMap ReadMember(MetadataToken token, ReadingContext context)
        {
            return new EncMap(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadUInt32(),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, EncMap member)
        {
            member.MetadataRow.Column1 = member.Token;
        }

        protected override void WriteMember(WritingContext context, EncMap member)
        {
            context.Writer.WriteUInt32(member.MetadataRow.Column1);
        }
    }

    public class EncMap : MetadataMember<MetadataRow<uint>>
    {
        internal EncMap(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            Token = row.Column1;
        }

        public uint Token
        {
            get;
            set;
        }
    }
}
