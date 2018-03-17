using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class ManifestResource : MetadataMember<MetadataRow<uint, ManifestResourceAttributes, uint, uint>>, IHasCustomAttribute
    {
        private readonly LazyValue<byte[]> _data;
        private readonly LazyValue<string> _name;
        private readonly LazyValue<IImplementation> _implementation;

        public ManifestResource(string name, ManifestResourceAttributes attributes, byte[] data)
            : base(new MetadataToken(MetadataTokenType.ManifestResource))
        {
            Attributes = attributes;
            _name = new LazyValue<string>(name);
            _data = new LazyValue<byte[]>(data);
            _implementation = new LazyValue<IImplementation>();

            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal ManifestResource(MetadataImage image, MetadataRow<uint, ManifestResourceAttributes, uint, uint> row)
            : base(row.MetadataToken)
        {
            Owner = image.Assembly;
            Offset = row.Column1;
            Attributes = row.Column2;

            _name = new LazyValue<string>(() => 
                image.Header.GetStream<StringStream>().GetStringByOffset(row.Column3));
            
            _implementation = new LazyValue<IImplementation>(() =>
            {
                var encoder = image.Header.GetStream<TableStream>().GetIndexEncoder(CodedIndex.Implementation);
                var implementationToken = encoder.DecodeIndex(row.Column4);
                IMetadataMember member;
                return image.TryResolveMember(implementationToken, out member)
                    ? (IImplementation) member
                    : null;
            });
            
            _data = new LazyValue<byte[]>(() => 
                IsEmbedded && Image != null
                ? Image.Header.NetDirectory.ResourcesManifest.GetResourceData(Offset)
                : null);

            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return Owner != null ? Owner.Image : null; }
        }

        public AssemblyDefinition Owner
        {
            get;
            internal set;
        }

        public uint Offset
        {
            get;
            set;
        }

        public ManifestResourceAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public IImplementation Implementation
        {
            get { return _implementation.Value; }
            set { _implementation.Value = value; }
        }

        public bool IsEmbedded
        {
            get { return Implementation == null; }
        }

        public byte[] Data
        {
            get { return _data.Value; }
            set { _data.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }
    }
}
