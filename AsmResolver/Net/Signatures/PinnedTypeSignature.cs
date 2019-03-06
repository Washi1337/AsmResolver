using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class PinnedTypeSignature : TypeSpecificationSignature
    {
        public static PinnedTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        public static PinnedTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new PinnedTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public PinnedTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType => ElementType.Pinned;

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte) +
                   BaseType.GetPhysicalLength(buffer) +
                   base.GetPhysicalLength(buffer);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            BaseType.Write(buffer, writer);
            base.Write(buffer, writer);
        }
    }
}
