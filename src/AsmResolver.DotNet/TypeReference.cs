using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to a type defined in a .NET assembly.
    /// </summary>
    public partial class TypeReference :
        MetadataMember,
        ITypeDefOrRef,
        IResolutionScope
    {
        private readonly object _lock = new();
        private readonly LazyVariable<TypeReference, Utf8String?> _namespace;

        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes a new empty type reference.
        /// </summary>
        /// <param name="token">The token of the type reference.</param>
        protected TypeReference(MetadataToken token)
            : base(token)
        {
            _namespace = new LazyVariable<TypeReference, Utf8String?>(x => x.GetNamespace());
        }

        /// <summary>
        /// Creates a new reference to a type.
        /// </summary>
        /// <param name="scope">The scope that defines the type.</param>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        /// <remarks>
        /// The resulting type will inherit the context module from <paramref name="scope"/>.
        /// </remarks>
        public TypeReference(IResolutionScope? scope, Utf8String? ns, Utf8String? name)
            : this(new MetadataToken(TableIndex.TypeRef, 0))
        {
            Scope = scope;
            ContextModule = scope?.ContextModule; // Assume the scope defines the module context.
            Namespace = ns;
            Name = name;
        }

        /// <summary>
        /// Creates a new reference to a type.
        /// </summary>
        /// <param name="contextModule">The module that references the type.</param>
        /// <param name="scope">The scope that defines the type.</param>
        /// <param name="ns">The namespace the type resides in.</param>
        /// <param name="name">The name of the type.</param>
        public TypeReference(ModuleDefinition? contextModule, IResolutionScope? scope, Utf8String? ns, Utf8String? name)
            : this(new MetadataToken(TableIndex.TypeRef, 0))
        {
            Scope = scope;
            ContextModule = contextModule;
            Namespace = ns;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the referenced type.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the type reference table.
        /// </remarks>
        [LazyProperty]
        public partial Utf8String? Name
        {
            get;
            set;
        }

        string? INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets the namespace the type is residing in.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Namespace column in the type definition table.
        /// </remarks>
        public Utf8String? Namespace
        {
            get => _namespace.GetValue(this);
            set => _namespace.SetValue(Utf8String.IsNullOrEmpty(value) ? null : value);
            // According to the specification, the namespace should always be null or non-empty.
        }

        string? ITypeDescriptor.Namespace => Namespace;

        /// <inheritdoc />
        public string FullName => MemberNameGenerator.GetTypeFullName(this);

        /// <inheritdoc />
        [LazyProperty]
        public partial IResolutionScope? Scope
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool IsValueType => Resolve()?.IsValueType ?? false;

        /// <inheritdoc />
        public ModuleDefinition? ContextModule
        {
            // Note: We cannot make this a computed property that returns `Scope.ContextModule`, because a TypeRef's
            // scope can be null and still be "valid" (albeit not according to spec). In such a case the runtime
            // assumes it references a type in the current module. We therefore have to keep track of it separately.
            get;
            protected set;
        }

        AssemblyDescriptor? IResolutionScope.GetAssembly() => Scope?.GetAssembly();

        /// <inheritdoc />
        public ITypeDefOrRef? DeclaringType => Scope as ITypeDefOrRef;

        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

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

        ITypeDefOrRef ITypeDescriptor.ToTypeDefOrRef() => this;

        /// <inheritdoc />
        public TypeSignature ToTypeSignature() => ToTypeSignature(IsValueType);

        /// <inheritdoc />
        public TypeSignature ToTypeSignature(bool isValueType)
        {
            return ContextModule?.CorLibTypeFactory.FromType(this) as TypeSignature
                   ?? new TypeDefOrRefSignature(this, isValueType);
        }

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module) =>
            ContextModule == module && (Scope?.IsImportedInModule(module) ?? false);

        /// <summary>
        /// Imports the type reference using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported type.</returns>
        public ITypeDefOrRef ImportWith(ReferenceImporter importer) => importer.ImportType(this);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <inheritdoc />
        public TypeDefinition? Resolve() => ContextModule is { } context ? Resolve(context) : null;

        /// <inheritdoc />
        public TypeDefinition? Resolve(ModuleDefinition context) => context.MetadataResolver.ResolveType(this);

        IMemberDefinition? IMemberDescriptor.Resolve() => Resolve();

        IMemberDefinition? IMemberDescriptor.Resolve(ModuleDefinition context) => Resolve(context);

        /// <summary>
        /// Obtains the name of the type reference.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the namespace of the type reference.
        /// </summary>
        /// <returns>The namespace.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Namespace"/> property.
        /// </remarks>
        protected virtual Utf8String? GetNamespace() => null;

        /// <summary>
        /// Obtains the scope of the type reference.
        /// </summary>
        /// <returns>The scope.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Scope"/> property.
        /// </remarks>
        protected virtual IResolutionScope? GetScope() => null;

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
