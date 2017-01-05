using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class PointerTypeSignature : TypeSpecificationSignature
    {
        public new static PointerTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new PointerTypeSignature(TypeSignature.FromReader(header, reader))
            {
                StartOffset = position
            };
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
