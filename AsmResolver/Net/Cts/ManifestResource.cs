using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a single manifest resource file either embedded into the .NET assembly, or put into a separate file.
    /// In this case, it contains also a reference to the file the resource is located in.
    /// </summary>
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
                return image.TryResolveMember(implementationToken, out var member)
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
        public override MetadataImage Image => Owner?.Image;

        /// <summary>
        /// Gets the owner of the resource.
        /// </summary>
        public AssemblyDefinition Owner
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the offset into the file where the resource data begins.
        /// 
        /// When <see cref="Implementation"/> is <c>null</c>, this offset is relative to the start address of the
        /// managed resource directory found in the PE. Otherwise, it is an offset into the file specified by this
        /// property. 
        /// </summary>
        public uint Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the resource.
        /// </summary>
        public ManifestResourceAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets or sets the file the resource is located in. When this property is <c>null</c>, the resource is
        /// located in the current .NET assembly image.
        /// </summary>
        public IImplementation Implementation
        {
            get => _implementation.Value;
            set => _implementation.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether the resource data is embedded into the current .NET assembly image.
        /// </summary>
        public bool IsEmbedded => Implementation == null;

        /// <summary>
        /// Gets or sets the data stored in the resource.
        /// </summary>
        public byte[] Data
        {
            get => _data.Value;
            set => _data.Value = value;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }
    }
}
