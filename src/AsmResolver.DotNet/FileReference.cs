using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to an external file that a .NET module depends on.
    /// </summary>
    public partial class FileReference :
        MetadataMember,
        IImplementation,
        IManagedEntryPoint,
        IOwnedCollectionElement<ModuleDefinition>
    {
        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes the file reference with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected FileReference(MetadataToken token)
            : base(token)
        {
        }

        /// <summary>
        /// Creates a new reference to an external file.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="attributes">The attributes associated to the reference.</param>
        public FileReference(Utf8String? name, FileAttributes attributes)
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
        [LazyProperty]
        public partial Utf8String? Name
        {
            get;
            set;
        }

        string? INameProvider.Name => Name;

        /// <inheritdoc />
        public string FullName => Name ?? NullName;

        /// <inheritdoc />
        public ModuleDefinition? ContextModule
        {
            get;
            private set;
        }

        ModuleDefinition? IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => ContextModule;
            set => ContextModule = value;
        }

        /// <summary>
        /// Gets or sets the checksum of the referenced file.
        /// </summary>
        [LazyProperty]
        public partial byte[]? HashValue
        {
            get;
            set;
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

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module) => ContextModule == module;

        /// <summary>
        /// Imports the file using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported file reference.</returns>
        public FileReference ImportWith(ReferenceImporter importer) =>
            (FileReference) importer.ImportImplementation(this);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

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
