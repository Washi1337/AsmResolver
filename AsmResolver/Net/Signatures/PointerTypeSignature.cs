using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class PointerTypeSignature : TypeSpecificationSignature
    {
        public static PointerTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        public static PointerTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new PointerTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public PointerTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType => ElementType.Ptr;

        public override string Name => BaseType.Name + "*";
    }
}
