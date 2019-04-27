using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class SentinelTypeSignature : TypeSpecificationSignature
    {
        public static SentinelTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        public static SentinelTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new SentinelTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public SentinelTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType => ElementType.Sentinel;
        
        public override TypeSignature InstantiateGenericTypes(IGenericContext context)
        {
            return new SentinelTypeSignature(BaseType.InstantiateGenericTypes(context));
        }
    }
}
