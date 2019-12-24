using AsmResolver.DotNet.Blob;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type that allows for assigning metadata tokens to type signatures stored in the blob stream. 
    /// </summary>
    public class TypeSpecification : IMetadataMember, ITypeDescriptor
    {
        private readonly LazyVariable<TypeSignature> _signature;
        
        /// <summary>
        /// Initializes an empty type specification.
        /// </summary>
        /// <param name="token">The token of the type specification.</param>
        protected TypeSpecification(MetadataToken token)
        {
            MetadataToken = token;
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

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
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
        public string Name => Signature.Name;

        /// <inheritdoc />
        public string Namespace => Signature.Namespace;
        
        /// <inheritdoc />
        public string FullName => Signature.FullName;

        /// <inheritdoc />
        public ModuleDefinition Module => Signature.Module;

        /// <inheritdoc />
        public IResolutionScope Scope => Signature.Scope;

        /// <inheritdoc />
        public ITypeDescriptor DeclaringType => Signature.DeclaringType;

        /// <inheritdoc />
        public bool IsValueType => Signature.IsValueType;

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
    }
}