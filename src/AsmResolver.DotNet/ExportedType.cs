using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type definition that was exported to another external .NET module.
    /// </summary>
    public class ExportedType :
        MetadataMember,
        IImplementation,
        ITypeDescriptor,
        IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<ExportedType, Utf8String?> _name;
        private readonly LazyVariable<ExportedType, Utf8String?> _namespace;
        private readonly LazyVariable<ExportedType, IImplementation?> _implementation;

        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes an exported type with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected ExportedType(MetadataToken token)
            : base(token)
        {
            _name = new LazyVariable<ExportedType, Utf8String?>(x => x.GetName());
            _namespace = new LazyVariable<ExportedType, Utf8String?>(x => x.GetNamespace());
            _implementation = new LazyVariable<ExportedType, IImplementation?>(x => x.GetImplementation());
        }

        /// <summary>
        /// Creates a new exported type reference.
        /// </summary>
        /// <param name="implementation">The file containing the type.</param>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        public ExportedType(IImplementation? implementation, Utf8String? ns, Utf8String? name)
            : this(new MetadataToken(TableIndex.ExportedType, 1))
        {
            Implementation = implementation;
            Namespace = ns;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the type.
        /// </summary>
        public TypeAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a hint to the row identifier of the type definition in the external module.
        /// </summary>
        public uint TypeDefId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the exported type table.
        /// </remarks>
        public Utf8String? Name
        {
            get => _name.GetValue(this);
            set => _name.SetValue(value);
        }

        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets the namespace of the type.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Namespace column in the exported type table.
        /// </remarks>
        public Utf8String? Namespace
        {
            get => _namespace.GetValue(this);
            set => _namespace.SetValue(value);
        }

        string? ITypeDescriptor.Namespace => Namespace;

        /// <inheritdoc />
        public string FullName => MemberNameGenerator.GetTypeFullName(this);

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
        /// Gets or sets the new location this type is exported to.
        /// </summary>
        public IImplementation? Implementation
        {
            get => _implementation.GetValue(this);
            set => _implementation.SetValue(value);
        }

        /// <summary>
        /// When this exported type is nested, gets the enclosing type.
        /// </summary>
        public ExportedType? DeclaringType => Implementation as ExportedType;

        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

        /// <inheritdoc />
        public IResolutionScope? Scope => ContextModule;

        /// <inheritdoc />
        public virtual bool HasCustomAttributes => CustomAttributes.Count > 0;

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
        public bool IsValueType => Resolve()?.IsValueType ?? false;

        /// <inheritdoc />
        public TypeDefinition? Resolve() => ContextModule is { } context ? Resolve(context) : null;

        /// <inheritdoc />
        public TypeDefinition? Resolve(ModuleDefinition context) => context.MetadataResolver.ResolveType(this);

        IMemberDefinition? IMemberDescriptor.Resolve() => Resolve();

        IMemberDefinition? IMemberDescriptor.Resolve(ModuleDefinition context) => Resolve(context);

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module)
        {
            return ContextModule == module && (Implementation?.IsImportedInModule(module) ?? false);
        }

        /// <summary>
        /// Imports the exported type using the provided importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported type.</returns>
        public ExportedType ImportWith(ReferenceImporter importer) => importer.ImportType(this);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <inheritdoc />
        public ITypeDefOrRef ToTypeDefOrRef() => new TypeReference(ContextModule, Scope, Namespace, Name);

        /// <inheritdoc />
        public TypeSignature ToTypeSignature() => new TypeDefOrRefSignature(ToTypeDefOrRef());

        /// <summary>
        /// Obtains the namespace of the exported type.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Namespace"/> property.
        /// </remarks>
        protected virtual Utf8String? GetNamespace() => null;

        /// <summary>
        /// Obtains the name of the exported type.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the implementation of the exported type.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Implementation"/> property.
        /// </remarks>
        protected virtual IImplementation? GetImplementation() => null;

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
        public override string ToString() => FullName;
    }
}
