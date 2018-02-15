using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class SzArrayTypeSignature : TypeSpecificationSignature
    {
        public new static SzArrayTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new SzArrayTypeSignature(TypeSignature.FromReader(image, reader))
            {
                StartOffset = position
            };
        }

        public SzArrayTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType
        {
            get { return ElementType.SzArray; }
        }

        public override string Name
        {
            get { return BaseType.Name + "[]"; }
        }
    }
}
