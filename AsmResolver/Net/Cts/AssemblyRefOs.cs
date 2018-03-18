using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class AssemblyRefOs : MetadataMember<MetadataRow<uint, uint,uint,uint>>
    {
        private readonly LazyValue<AssemblyReference> _reference;
        private MetadataImage _image;

        public AssemblyRefOs(AssemblyReference reference, uint platformId, uint majorVersion, uint minorVersion)
            : base(new MetadataToken(MetadataTokenType.AssemblyRefOs))
        {
            PlatformId = platformId;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            _reference = new LazyValue<AssemblyReference>(reference);
        }
        
        internal AssemblyRefOs(MetadataImage image, MetadataRow<uint, uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            PlatformId = row.Column1;
            MajorVersion = row.Column2;
            MinorVersion = row.Column3;
            _reference = new LazyValue<AssemblyReference>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.AssemblyRef);
                MetadataRow referenceRow;
                return table.TryGetRow((int) (row.Column4 - 1), out referenceRow)
                    ? (AssemblyReference) table.GetMemberFromRow(image, referenceRow)
                    : null;
            });
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _reference.IsInitialized && _reference.Value != null ? _reference.Value.Image : _image; }
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
            get { return _reference.Value;}
            internal set
            {
                _reference.Value = value;
                _image = null;
            }
        }
    }
}
