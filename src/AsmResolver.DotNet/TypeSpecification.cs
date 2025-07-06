using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type that allows for assigning metadata tokens to type signatures stored in the blob stream.
    /// </summary>
    public class TypeSpecification :
        MetadataMember,
        ITypeDefOrRef
    {
        private readonly LazyVariable<TypeSpecification, TypeSignature?> _signature;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Initializes an empty type specification.
        /// </summary>
        /// <param name="token">The token of the type specification.</param>
        protected TypeSpecification(MetadataToken token)
            : base(token)
        {
            _signature = new LazyVariable<TypeSpecification, TypeSignature?>(x => x.GetSignature());
        }

        /// <summary>
        /// Creates a new type specification.
        /// </summary>
        /// <param name="signature">The type signature to assign a metadata token.</param>
        public TypeSpecification(TypeSignature? signature)
            : this(new MetadataToken(TableIndex.TypeSpec, 0))
        {
            Signature = signature;
        }

        /// <summary>
        /// Gets or sets the type signature that this type specification is referencing.
        /// </summary>
        public TypeSignature? Signature
        {
            get => _signature.GetValue(this);
            set => _signature.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the name of the referenced type.
        /// </summary>
        public Utf8String Name => Signature?.Name ?? TypeSignature.NullTypeToString;

        string INameProvider.Name => Name;

        /// <summary>
        /// Gets or sets the namespace the type is residing in.
        /// </summary>
        public Utf8String? Namespace => Signature?.Namespace;

        string? ITypeDescriptor.Namespace => Namespace;

        /// <inheritdoc />
        public string FullName => MemberNameGenerator.GetTypeFullName(this);

        /// <inheritdoc />
        public ModuleDefinition? ContextModule => Signature?.ContextModule;

        /// <inheritdoc />
        public IResolutionScope? Scope => Signature?.Scope;

        /// <inheritdoc />
        public ITypeDefOrRef? DeclaringType => Signature?.DeclaringType as ITypeDefOrRef;

        /// <inheritdoc />
        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

        /// <inheritdoc />
        public bool IsValueType => Signature?.IsValueType ?? false;

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

        ITypeDefOrRef ITypeDescriptor.ToTypeDefOrRef() => this;

        /// <inheritdoc />
        public TypeSignature ToTypeSignature() =>
            Signature ?? throw new ArgumentException("Signature embedded into the type specification is null.");

        /// <inheritdoc />
        public TypeSignature ToTypeSignature(bool isValueType) => ToTypeSignature();

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module) => Signature?.IsImportedInModule(module) ?? false;

        /// <summary>
        /// Imports the type specification using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported type.</returns>
        public ITypeDefOrRef ImportWith(ReferenceImporter importer) => (TypeSpecification) importer.ImportType(this);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <inheritdoc />
        public TypeDefinition? Resolve() => ContextModule is { } context ? Resolve(context) : null;

        /// <inheritdoc />
        public TypeDefinition? Resolve(ModuleDefinition context) => ContextModule?.MetadataResolver.ResolveType(this);

        IMemberDefinition? IMemberDescriptor.Resolve() => Resolve();

        IMemberDefinition? IMemberDescriptor.Resolve(ModuleDefinition context) => Resolve(context);

        /// <summary>
        /// Obtains the signature the type specification is referencing.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual TypeSignature? GetSignature() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;

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
