using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single manifest resource file either embedded into the .NET assembly, or put into a separate file.
    /// In this case, it contains also a reference to the file the resource is located in.
    /// </summary>
    public class ManifestResource :
        MetadataMember,
        IHasCustomAttribute,
        IModuleProvider,
        IMetadataDefinition,
        IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<ManifestResource, Utf8String?> _name;
        private readonly LazyVariable<ManifestResource, IImplementation?> _implementation;
        private readonly LazyVariable<ManifestResource, ISegment?> _embeddedData;

        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes the <see cref="ManifestResource"/> with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected ManifestResource(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<ManifestResource, Utf8String?>(x => x.GetName());
            _implementation = new LazyVariable<ManifestResource, IImplementation?>(x => x.GetImplementation());
            _embeddedData = new LazyVariable<ManifestResource, ISegment?>(x => x.GetEmbeddedDataSegment());
        }

        /// <summary>
        /// Creates a new external manifest resource.
        /// </summary>
        /// <param name="name">The name of the resource</param>
        /// <param name="attributes">The attributes of the resource.</param>
        /// <param name="implementation">The location of the resource data.</param>
        /// <param name="offset">The offset within the file referenced by <paramref name="implementation"/> where the data starts.</param>
        public ManifestResource(Utf8String? name, ManifestResourceAttributes attributes, IImplementation? implementation, uint offset)
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
        public ManifestResource(Utf8String? name, ManifestResourceAttributes attributes, ISegment? data)
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
            get => _name.GetValue(this);
            set => _name.SetValue(value);
        }

        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets the implementation indicating the file containing the resource data.
        /// </summary>
        public IImplementation? Implementation
        {
            get => _implementation.GetValue(this);
            set => _implementation.SetValue(value);
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
            get => _embeddedData.GetValue(this);
            set => _embeddedData.SetValue(value);
        }

        /// <summary>
        /// Gets the module that this manifest resource reference is stored in.
        /// </summary>
        public ModuleDefinition? DeclaringModule
        {
            get;
            private set;
        }

        ModuleDefinition? IModuleProvider.ContextModule => DeclaringModule;

        /// <inheritdoc />
        ModuleDefinition? IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => DeclaringModule;
            set => DeclaringModule = value;
        }

        /// <inheritdoc />
        public virtual bool HasCustomAttributes => CustomAttributesInternal is { Count: > 0 };

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (CustomAttributesInternal is null)
                    Interlocked.CompareExchange(ref CustomAttributesInternal, GetCustomAttributes(), null);
                return CustomAttributesInternal;
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
        [Obsolete("Use TryGetReader instead.")]
        public BinaryStreamReader? GetReader()
        {
            // TODO: resolve external resources.

            return EmbeddedDataSegment is IReadableSegment readableSegment
                ? readableSegment.CreateReader()
                : null;
        }

        /// <summary>
        /// Gets the reader of stored data in the manifest resource.
        /// </summary>
        public bool TryGetReader(out BinaryStreamReader reader)
        {
            // TODO: resolve external resources.

            if (EmbeddedDataSegment is IReadableSegment readableSegment)
            {
                reader = readableSegment.CreateReader();
                return true;
            }

            reader = default;
            return false;
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
