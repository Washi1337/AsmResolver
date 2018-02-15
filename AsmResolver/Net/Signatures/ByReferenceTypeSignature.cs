using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class ByReferenceTypeSignature : TypeSpecificationSignature
    {
        public new static ByReferenceTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new ByReferenceTypeSignature(TypeSignature.FromReader(image, reader))
            {
                StartOffset = position
            };
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
