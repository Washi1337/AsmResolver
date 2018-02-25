using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class PointerTypeSignature : TypeSpecificationSignature
    {
        public new static PointerTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new PointerTypeSignature(TypeSignature.FromReader(image, reader));
        }

        public PointerTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType
        {
            get { return ElementType.Ptr; }
        }
        
        public override string Name
        {
            get { return BaseType.Name + "*"; }
        }
    }
}
