using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class SzArrayTypeSignature : TypeSpecificationSignature
    {
        public static SzArrayTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        public static SzArrayTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new SzArrayTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public SzArrayTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType => ElementType.SzArray;

        public override string Name => BaseType.Name + "[]";
    }
}
