using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class ByReferenceTypeSignature : TypeSpecificationSignature
    {
        public new static ByReferenceTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return new ByReferenceTypeSignature(TypeSignature.FromReader(image, reader));
        }

        public ByReferenceTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType
        {
            get { return ElementType.ByRef; }
        }

        public override string Name
        {
            get { return BaseType.Name + '&'; }
        }
    }
}
