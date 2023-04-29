using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to a generic method that is instantiated with type arguments.
    /// </summary>
    public class MethodSpecification : MetadataMember, IMethodDescriptor, IHasCustomAttribute
    {
        private readonly LazyVariable<MethodSpecification, IMethodDefOrRef?> _method;
        private readonly LazyVariable<MethodSpecification, GenericInstanceMethodSignature?> _signature;
        private IList<CustomAttribute>? _customAttributes;

        /// <summary>
        /// Creates a new empty method specification.
        /// </summary>
        /// <param name="token">The token of the specification.</param>
        protected MethodSpecification(MetadataToken token)
            : base(token)
        {
            _method = new LazyVariable<MethodSpecification, IMethodDefOrRef?>(x => x.GetMethod());
            _signature = new LazyVariable<MethodSpecification, GenericInstanceMethodSignature?>(x => x.GetSignature());
        }

        /// <summary>
        /// Creates a new reference to a generic instantiation of a method.
        /// </summary>
        /// <param name="method">The method to instantiate.</param>
        /// <param name="signature">The instantiation signature.</param>
        public MethodSpecification(IMethodDefOrRef? method, GenericInstanceMethodSignature? signature)
            : this(new MetadataToken(TableIndex.MethodSpec,0))
        {
            Method = method;
            Signature = signature;
        }

        /// <summary>
        /// Gets or sets the method that was instantiated.
        /// </summary>
        public IMethodDefOrRef? Method
        {
            get => _method.GetValue(this);
            set => _method.SetValue(value);
        }

        MethodSignature? IMethodDescriptor.Signature => Method?.Signature;

        /// <summary>
        /// Gets or sets the generic instantiation of the method.
        /// </summary>
        public GenericInstanceMethodSignature? Signature
        {
            get => _signature.GetValue(this);
            set => _signature.SetValue(value);
        }


        /// <summary>
        /// Gets or sets the name of the method specification.
        /// </summary>
        public Utf8String Name => Method?.Name ?? NullName;

        string? INameProvider.Name => Name;

        /// <inheritdoc />
        public string FullName => MemberNameGenerator.GetMethodFullName(this);

        /// <inheritdoc />
        public ModuleDefinition? Module => Method?.Module;

        /// <summary>
        /// Gets the declaring type of the method.
        /// </summary>
        public ITypeDefOrRef? DeclaringType => Method?.DeclaringType;

        ITypeDescriptor? IMemberDescriptor.DeclaringType => DeclaringType;

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
        public MethodDefinition? Resolve() => Method?.Resolve();

        /// <inheritdoc />
        public bool IsImportedInModule(ModuleDefinition module)
        {
            return (Method?.IsImportedInModule(module) ?? false)
                   && (Signature?.IsImportedInModule(module) ?? false);
        }

        /// <summary>
        /// Imports the method specification using the provided reference importer.
        /// </summary>
        /// <param name="importer">The reference importer to use.</param>
        /// <returns>The imported method specification.</returns>
        public MethodSpecification ImportWith(ReferenceImporter importer) => importer.ImportMethod(this);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        IMemberDefinition? IMemberDescriptor.Resolve() => Resolve();

        /// <summary>
        /// Obtains the instantiated method.
        /// </summary>
        /// <returns>The method.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Method"/> property.
        /// </remarks>
        protected virtual IMethodDefOrRef? GetMethod() => null;

        /// <summary>
        /// Obtains the method instantiation signature.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual GenericInstanceMethodSignature? GetSignature() => null;

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
