using System;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a type that is specified by a blob signature.
    /// </summary>
    public class TypeSpecification : MetadataMember<MetadataRow<uint>>, ITypeDefOrRef
    {
        private readonly LazyValue<TypeSignature> _signature;
        public TypeSpecification(TypeSignature signature)
            : base(new MetadataToken(MetadataTokenType.TypeSpec))
        {
            _signature = new LazyValue<TypeSignature>(signature);
            CustomAttributes = new CustomAttributeCollection(this);
        }

        public TypeSpecification(MetadataImage image, MetadataRow<uint> row)
            : base(row.MetadataToken)
        {
            _signature = new LazyValue<TypeSignature>(() => 
                TypeSignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column1), true));
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image => Signature?.ResolutionScope?.Image;

        /// <summary>
        /// Gets or sets the type signature that is referred to by this specification.
        /// </summary>
        public TypeSignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public string Name => Signature.Name;

        string IMemberReference.Name
        {
            get => Name;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the namespace of the type.
        /// </summary>
        public string Namespace => Signature.Namespace;

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor => DeclaringType;

        /// <inheritdoc />
        public IResolutionScope ResolutionScope => Signature.ResolutionScope;

        /// <inheritdoc />
        public virtual string FullName => Signature.FullName;

        /// <inheritdoc />
        public bool IsValueType => Signature.IsValueType;

        public ITypeDescriptor GetElementType()
        {
            return Signature.GetElementType();
        }

        /// <inheritdoc />
        public ITypeDefOrRef DeclaringType => ResolutionScope as ITypeDefOrRef;

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <summary>
        /// Resolves the type embedded in the signature to its definition.
        /// </summary>
        /// <returns>The type definition.</returns>
        /// <exception cref="MemberResolutionException">Occurs when the resolution fails.</exception>
        public TypeDefinition Resolve()
        {
            if (Image?.MetadataResolver == null)
                throw new MemberResolutionException(this);
            return Image.MetadataResolver.ResolveType(this);
        }

        IMetadataMember IResolvable.Resolve()
        {
            return Resolve();
        }

        public override string ToString()
        {
            return Signature.FullName;
        }
    }
}
