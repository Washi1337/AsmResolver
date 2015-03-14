using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class ByReferenceTypeSignature : TypeSpecificationSignature
    {
        public new static ByReferenceTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new ByReferenceTypeSignature(TypeSignature.FromReader(header, reader));
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
