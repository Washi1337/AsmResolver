using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyRefOsTable : MetadataTable<AssemblyRefOs>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyRefOs; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   sizeof (uint) +
                   sizeof (uint) +
                   (uint)TableStream.GetTable<AssemblyReference>().IndexSize;
        }

        protected override AssemblyRefOs ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new AssemblyRefOs(Header, token, new MetadataRow<uint, uint, uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadUInt32(),
                Column3 = reader.ReadUInt32(),
                Column4 = reader.ReadIndex(TableStream.GetTable<AssemblyReference>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, AssemblyRefOs member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.PlatformId;
            row.Column2 = member.MajorVersion;
            row.Column3 = member.MinorVersion;
            row.Column4 = member.Reference.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, AssemblyRefOs member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            writer.WriteUInt32(row.Column1);
            writer.WriteUInt32(row.Column2);
            writer.WriteUInt32(row.Column3);
            writer.WriteIndex(TableStream.GetTable<AssemblyReference>().IndexSize, row.Column4);
        }
    }

    public class AssemblyRefOs : MetadataMember<MetadataRow<uint, uint,uint,uint>>
    {
        internal AssemblyRefOs(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            PlatformId = row.Column1;
            MajorVersion = row.Column2;
            MinorVersion = row.Column3;
            Reference = header.GetStream<TableStream>().GetTable<AssemblyReference>()[(int)(row.Column4 - 1)];

        }

        public uint PlatformId
        {
            get;
            set;
        }

        public uint MajorVersion
        {
            get;
            set;
        }

        public uint MinorVersion
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
