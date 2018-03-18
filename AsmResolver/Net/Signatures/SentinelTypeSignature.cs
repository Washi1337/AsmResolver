using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class SentinelTypeSignature : TypeSpecificationSignature
    {
        public new static SentinelTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return new SentinelTypeSignature(TypeSignature.FromReader(image, reader));
        }

        public SentinelTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType
        {
            get { return ElementType.Sentinel; }
        }

    }
}
