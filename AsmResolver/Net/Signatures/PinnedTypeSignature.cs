using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class PinnedTypeSignature : TypeSpecificationSignature
    {
        public new static PinnedTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            long position = reader.StartPosition;
            return new PinnedTypeSignature(TypeSignature.FromReader(image, reader))
            {
                StartOffset = reader.Position,
            };
        }

        public PinnedTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType
        {
            get { return ElementType.Pinned; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte) +
                   BaseType.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteByte((byte)ElementType);
            BaseType.Write(context);
        }
    }
}
