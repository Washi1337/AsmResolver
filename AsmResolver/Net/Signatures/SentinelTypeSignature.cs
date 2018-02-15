using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class SentinelTypeSignature : TypeSpecificationSignature
    {
        public new static SentinelTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new SentinelTypeSignature(TypeSignature.FromReader(image, reader))
            {
                StartOffset = position
            };
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
