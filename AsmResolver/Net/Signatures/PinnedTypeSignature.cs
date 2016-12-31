using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class PinnedTypeSignature : TypeSpecificationSignature
    {
        public new static PinnedTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new PinnedTypeSignature(TypeSignature.FromReader(header, reader));
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
