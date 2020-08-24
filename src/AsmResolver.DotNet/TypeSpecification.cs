using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Types;
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
        private readonly LazyVariable<TypeSignature> _signature;
        private IList<CustomAttribute> _customAttributes;

        /// <summary>
        /// Initializes an empty type specification.
        /// </summary>
        /// <param name="token">The token of the type specification.</param>
        protected TypeSpecification(MetadataToken token)
            : base(token)
        {
            _signature = new LazyVariable<TypeSignature>(GetSignature);
        }

        /// <summary>
        /// Creates a new type specification.
        /// </summary>
        /// <param name="signature">The type signature to assign a metadata token.</param>
        public TypeSpecification(TypeSignature signature)
            : this(new MetadataToken(TableIndex.TypeSpec, 0))
        {
            Signature = signature;
        }

        /// <summary>
        /// Gets or sets the type signature that this type specification is referencing.
        /// </summary>
        public TypeSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public string Name => Signature?.Name ?? TypeSignature.NullTypeToString;

        /// <inheritdoc />
        public string Namespace => Signature?.Namespace;
        
        /// <inheritdoc />
        public string FullName => this.GetTypeFullName();

        /// <inheritdoc />
        public ModuleDefinition Module => Signature?.Module;

        /// <inheritdoc />
        public IResolutionScope Scope => Signature?.Scope;

        /// <inheritdoc />
        public ITypeDefOrRef DeclaringType => Signature?.DeclaringType as ITypeDefOrRef;

        /// <inheritdoc />
        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

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
        public TypeSignature ToTypeSignature() => Signature;

        /// <inheritdoc />
        public TypeDefinition Resolve() => Module?.MetadataResolver?.ResolveType(this);
        
        IMemberDefinition IMemberDescriptor.Resolve() => Resolve();
        
        /// <summary>
        /// Obtains the signature the type specification is referencing.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual TypeSignature GetSignature() => null;

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