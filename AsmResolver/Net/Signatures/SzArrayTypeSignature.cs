using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class SzArrayTypeSignature : TypeSpecificationSignature
    {
        public new static SzArrayTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new SzArrayTypeSignature(TypeSignature.FromReader(header, reader));
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
