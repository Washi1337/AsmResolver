using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single manifest resource file either embedded into the .NET assembly, or put into a separate file.
    /// In this case, it contains also a reference to the file the resource is located in.
    /// </summary>
    public class ManifestResource :
        MetadataMember,
        INameProvider,
        IHasCustomAttribute,
        IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<Utf8String?> _name;
        private readonly LazyVariable<IImplementation?> _implementation;
        private readonly LazyVariable<ISegment?> _embeddedData;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes the <see cref="ManifestResource"/> with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected ManifestResource(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<Utf8String?>(GetName);
            _implementation = new LazyVariable<IImplementation?>(GetImplementation);
            _embeddedData = new LazyVariable<ISegment?>(GetEmbeddedDataSegment);
        }

        /// <summary>
        /// Creates a new external manifest resource.
        /// </summary>
        /// <param name="name">The name of the resource</param>
        /// <param name="attributes">The attributes of the resource.</param>
        /// <param name="implementation">The location of the resource data.</param>
        /// <param name="offset">The offset within the file referenced by <paramref name="implementation"/> where the data starts.</param>
        public ManifestResource(string? name, ManifestResourceAttributes attributes, IImplementation? implementation, uint offset)
            : this(new MetadataToken(TableIndex.ManifestResource, 0))
        {
            Name = name;
            Attributes = attributes;
            Implementation = implementation;
            Offset = offset;
        }

        /// <summary>
        /// Creates a new embedded manifest resource.
        /// </summary>
        /// <param name="name">The name of the repository.</param>
        /// <param name="attributes">The attributes of the resource.</param>
        /// <param name="data">The embedded resource data.</param>
        public ManifestResource(string? name, ManifestResourceAttributes attributes, ISegment? data)
            : this(new MetadataToken(TableIndex.ManifestResource, 0))
        {
            Name = name;
            Attributes = attributes;
            EmbeddedDataSegment = data;
        }

        /// <summary>
        /// Depending on the value of <see cref="Implementation"/>, gets or sets the (relative) offset the resource data
        /// starts at.
        /// </summary>
        public uint Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes associated with this resource.
        /// </summary>
        public ManifestResourceAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resource is public and exported by the .NET module.
        /// </summary>
        public bool IsPublic
        {
            get => Attributes == ManifestResourceAttributes.Public;
            set => Attributes = value ? ManifestResourceAttributes.Public : ManifestResourceAttributes.Private;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resource is private to the .NET module.
        /// </summary>
        public bool IsPrivate
        {
            get => Attributes == ManifestResourceAttributes.Private;
            set => Attributes = value ? ManifestResourceAttributes.Private : ManifestResourceAttributes.Public;
        }

        /// <summary>
        /// Gets or sets the name of the manifest resource.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the manifest resource table.
        /// </remarks>
        public Utf8String? Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets the implementation indicating the file containing the resource data.
        /// </summary>
        public IImplementation? Implementation
        {
            get => _implementation.Value;
            set => _implementation.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether the resource is embedded into the current module.
        /// </summary>
        public bool IsEmbedded => Implementation is null;

        /// <summary>
        /// When this resource is embedded into the current module, gets or sets the embedded resource data.
        /// </summary>
        public ISegment? EmbeddedDataSegment
        {
            get => _embeddedData.Value;
            set => _embeddedData.Value = value;
        }

        /// <summary>
        /// Gets the module that this manifest resource reference is stored in.
        /// </summary>
        public ModuleDefinition? Module
        {
            get;
            private set;
        }

        /// <inheritdoc />
        ModuleDefinition? IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => Module = value;
        }

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (_customAttributes is null)
                    Interlocked.CompareExchange(ref _customAttributes, GetCustomAttributes(), null);
                return _customAttributes;
            }
        }

        /// <summary>
        /// Gets the data stored in the manifest resource.
        /// </summary>
        /// <returns>The data, or <c>null</c> if no data was stored or if the external resource was not found.</returns>
        public byte[]? GetData()
        {
            // TODO: resolve external resources.

            return EmbeddedDataSegment is IReadableSegment readableSegment
                ? readableSegment.ToArray()
                : null;
        }

        /// <summary>
        /// Gets the reader of stored data in the manifest resource.
        /// </summary>
        /// <returns>The reader, or <c>null</c> if no data was stored or if the external resource was not found.</returns>
        public BinaryStreamReader? GetReader()
        {
            // TODO: resolve external resources.

            return EmbeddedDataSegment is IReadableSegment readableSegment
                ? readableSegment.CreateReader()
                : null;
        }

        /// <summary>
        /// Obtains the name of the manifest resource.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the implementation of this resource.
        /// </summary>
        /// <returns>The implementation.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Implementation"/> property.
        /// </remarks>
        protected virtual IImplementation? GetImplementation() => null;

        /// <summary>
        /// When the resource is embedded, obtains the contents of the manifest resource.
        /// </summary>
        /// <returns>The data, or <c>null</c> if the resource is not embedded.</returns>
        protected virtual ISegment? GetEmbeddedDataSegment() => null;

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);

        /// <inheritdoc />
        public override string ToString() => Name ?? NullName;
    }
}
