using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class AssemblyOs : MetadataMember<MetadataRow<uint, uint, uint>>
    {
        public AssemblyOs(uint platformId, uint majorVersion, uint minorVersion)
            : base(null, new MetadataToken(MetadataTokenType.AssemblyOs))
        {
            PlatformId = platformId;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        internal AssemblyOs(MetadataImage image, MetadataRow<uint, uint, uint> row)
            : base(image, row.MetadataToken)
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
