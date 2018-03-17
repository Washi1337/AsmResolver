using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
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
                TypeSignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column1)));
            CustomAttributes = new CustomAttributeCollection(this);
        }

        public override MetadataImage Image
        {
            get { return Signature != null && Signature.ResolutionScope != null ? Signature.ResolutionScope.Image : null; }
        }

        public TypeSignature Signature
        {
            get { return _signature.Value; }
            set { _signature.Value = value; }
        }

        public string Name
        {
            get { return Signature.Name; }
        }

        public string Namespace
        {
            get { return Signature.Namespace; }
        }

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor
        {
            get { return DeclaringType; }
        }

        public IResolutionScope ResolutionScope
        {
            get { return Signature.ResolutionScope; }
        }

        public virtual string FullName
        {
            get { return Signature.FullName; }
        }

        public bool IsValueType
        {
            get { return Signature.IsValueType; }
        }

        public ITypeDescriptor GetElementType()
        {
            return Signature.GetElementType();
        }

        public ITypeDefOrRef DeclaringType
        {
            get { return ResolutionScope as ITypeDefOrRef; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        public TypeDefinition Resolve()
        {
            if (Image == null || Image.MetadataResolver == null)
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
