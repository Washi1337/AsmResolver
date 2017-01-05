using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class SentinelTypeSignature : TypeSpecificationSignature
    {
        public new static SentinelTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new SentinelTypeSignature(TypeSignature.FromReader(header, reader))
            {
                StartOffset = position
            };
        }

        public SentinelTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        public override ElementType ElementType
        {
            get { return ElementType.Sentinel; }
        }

    }
}
