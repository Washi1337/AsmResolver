using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyRefProcessorTable : MetadataTable<AssemblyRefProcessor>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyRefProcessor; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   (uint)TableStream.GetTable<AssemblyReference>().IndexSize;
        }

        protected override AssemblyRefProcessor ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new AssemblyRefProcessor(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadIndex(TableStream.GetTable<AssemblyReference>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, AssemblyRefProcessor member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Processor;
            row.Column2 = member.Reference.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, AssemblyRefProcessor member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            writer.WriteUInt32(row.Column1);
            writer.WriteIndex(TableStream.GetTable<AssemblyReference>().IndexSize, row.Column2);
        }
    }

    public class AssemblyRefProcessor : MetadataMember<MetadataRow<uint,uint>>
    {
        internal AssemblyRefProcessor(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            Processor = row.Column1;
            Reference = header.GetStream<TableStream>().GetTable<AssemblyReference>()[(int)(row.Column1 - 1)];
        }

        public uint Processor
        {
            get;
            set;
        }

        public AssemblyReference Reference
        {
            get;
            set;
        }
    }
}
