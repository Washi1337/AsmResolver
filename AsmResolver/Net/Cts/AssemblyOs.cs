using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class AssemblyOs : MetadataMember<MetadataRow<uint, uint, uint>>
    {
        public AssemblyOs(uint platformId, uint majorVersion, uint minorVersion)
            : base(new MetadataToken(MetadataTokenType.AssemblyOs))
        {
            PlatformId = platformId;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        internal AssemblyOs(MetadataImage image, MetadataRow<uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            Assembly = image.Assembly;
            PlatformId = row.Column1;
            MajorVersion = row.Column2;
            MinorVersion = row.Column3;
        }

        public override MetadataImage Image
        {
            get { return Assembly != null ? Assembly.Image : null; }
        }

        public AssemblyDefinition Assembly
        {
            get;
            internal set;
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
