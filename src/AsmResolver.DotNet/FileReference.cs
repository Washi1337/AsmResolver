using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to an external file that a .NET module depends on.
    /// </summary>
    public class FileReference :
        MetadataMember,
        IImplementation,
        IManagedEntrypoint,
        IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<Utf8String?> _name;
        private readonly LazyVariable<byte[]?> _hashValue;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes the file reference with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected FileReference(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<Utf8String?>(GetName);
            _hashValue = new LazyVariable<byte[]?>(GetHashValue);
        }

        /// <summary>
        /// Creates a new reference to an external file.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="attributes">The attributes associated to the reference.</param>
        public FileReference(string? name, FileAttributes attributes)
            : this(new MetadataToken(TableIndex.File, 0))
        {
            Name = name;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the file reference.
        /// </summary>
        public FileAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the referenced file contains .NET metadata.
        /// </summary>
        public bool ContainsMetadata
        {
            get => !ContainsNoMetadata;
            set => ContainsNoMetadata = !value;
        }

        /// <summary>
        /// Gets or sets a value indicating the referenced file does not contain .NET metadat.
        /// </summary>
        public bool ContainsNoMetadata
        {
            get => Attributes == FileAttributes.ContainsNoMetadata;
            set => Attributes = (Attributes & ~FileAttributes.ContainsNoMetadata)
                                | (value ? FileAttributes.ContainsNoMetadata : 0);
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the file table.
        /// </remarks>
        public Utf8String? Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        string? INameProvider.Name => Name;

        /// <inheritdoc />
        public string FullName => Name ?? NullName;

        /// <inheritdoc />
        public ModuleDefinition? Module
        {
            get;
            private set;
        }

        ModuleDefinition? IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => Module = value;
        }

        /// <summary>
        /// Gets or sets the checksum of the referenced file.
        /// </summary>
        public byte[]? HashValue
        {
            get => _hashValue.Value;
            set => _hashValue.Value = value;
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

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module) => Module == module;

        /// <summary>
        /// Obtains the name of the referenced file.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the hash of the referenced file.
        /// </summary>
        /// <returns>The hash.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="HashValue"/> property.
        /// </remarks>
        protected virtual byte[]? GetHashValue() => null;

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);
    }
}
