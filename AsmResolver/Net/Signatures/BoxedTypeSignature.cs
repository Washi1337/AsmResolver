using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class BoxedTypeSignature : TypeSpecificationSignature
    {
        public new static BoxedTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return new BoxedTypeSignature(TypeSignature.FromReader(image, reader));
        }

        public BoxedTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType
        {
            get { return ElementType.Boxed; }
        }
    }
}
