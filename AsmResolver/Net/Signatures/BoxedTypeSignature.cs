using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class BoxedTypeSignature : TypeSpecificationSignature
    {
        public new static BoxedTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new BoxedTypeSignature(TypeSignature.FromReader(header, reader))
            {
                StartOffset = position
            };
        }

        public BoxedTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType
        {
            get { return ElementType.Boxed; }
        }
    }
}
