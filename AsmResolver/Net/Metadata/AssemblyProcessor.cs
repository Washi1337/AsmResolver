using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyProcessorTable : MetadataTable<AssemblyProcessor> 
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyProcessor; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint);
        }

        protected override AssemblyProcessor ReadMember(MetadataToken token, ReadingContext context)
        {
            return new AssemblyProcessor(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadUInt32(),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, AssemblyProcessor member)
        {
            member.MetadataRow.Column1 = member.Processor;
        }

        protected override void WriteMember(WritingContext context, AssemblyProcessor member)
        {
            context.Writer.WriteUInt32(member.MetadataRow.Column1);
        }
    }

    public class AssemblyProcessor : MetadataMember<MetadataRow<uint>>
    {
        internal AssemblyProcessor(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            Processor = row.Column1;
        }

        public uint Processor
        {
            get;
            set;
        }
    }
}
