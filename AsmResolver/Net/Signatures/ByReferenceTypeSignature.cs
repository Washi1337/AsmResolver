using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class ByReferenceTypeSignature : TypeSpecificationSignature
    {
        public static ByReferenceTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        public static ByReferenceTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new ByReferenceTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public ByReferenceTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType => ElementType.ByRef;

        public override string Name => BaseType.Name + '&';
    }
}
