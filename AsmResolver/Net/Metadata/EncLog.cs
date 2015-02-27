using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class EncLogTable : MetadataTable<EncLog>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.EncLog; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   sizeof (uint);
        }

        protected override EncLog ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new EncLog(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadUInt32(),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, EncLog member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Token;
            row.Column2 = member.FuncCode;
        }

        protected override void WriteMember(WritingContext context, EncLog member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            writer.WriteUInt32(row.Column1);
            writer.WriteUInt32(row.Column2);
        }
    }

    public class EncLog : MetadataMember<MetadataRow<uint, uint>>
    {
        internal EncLog(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            Token = row.Column1;
            FuncCode = row.Column2;
        }

        public uint Token
        {
            get;
            set;
        }

        public uint FuncCode
        {
            get;
            set;
        }
    }
}
