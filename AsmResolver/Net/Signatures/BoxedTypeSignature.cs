using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class BoxedTypeSignature : TypeSpecificationSignature
    {
        public static BoxedTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }
        
        public static BoxedTypeSignature FromReader(MetadataImage image, 
            IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            return new BoxedTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public BoxedTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType => ElementType.Boxed;
    }
}
