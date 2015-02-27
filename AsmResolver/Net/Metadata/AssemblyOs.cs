using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyOsTable : MetadataTable<AssemblyOs>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyOs; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   sizeof (uint) +
                   sizeof (uint);
        }

        protected override AssemblyOs ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new AssemblyOs(Header, token, new MetadataRow<uint, uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadUInt32(),
                Column3 = reader.ReadUInt32(),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, AssemblyOs member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.PlatformId;
            row.Column2 = member.MajorVersion;
            row.Column3 = member.MinorVersion;
        }

        protected override void WriteMember(WritingContext context, AssemblyOs member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            writer.WriteUInt32(row.Column1);
            writer.WriteUInt32(row.Column2);
            writer.WriteUInt32(row.Column3);
        }
    }

    public class AssemblyOs : MetadataMember<MetadataRow<uint,uint,uint>>
    {
        public AssemblyOs(uint platformId, uint majorVersion, uint minorVersion)
            : base(null, new MetadataToken(MetadataTokenType.AssemblyOs), new MetadataRow<uint, uint, uint>())
        {
            PlatformId = platformId;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        internal AssemblyOs(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint> row)
            : base(header, token, row)
        {
            PlatformId = row.Column1;
            MajorVersion = row.Column2;
            MinorVersion = row.Column3;
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
    }
}
