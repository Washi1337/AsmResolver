using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class SentinelTypeSignature : TypeSignature
    {
        public new static SentinelTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new SentinelTypeSignature(TypeSignature.FromReader(header, reader));
        }

        public SentinelTypeSignature(TypeSignature baseType)
        {
            BaseType = baseType;
        }

        public TypeSignature BaseType
        {
            get;
            set;
        }

        public override ElementType ElementType
        {
            get { return ElementType.Sentinel; }
        }

        public override string Name
        {
            get { return BaseType.Name; }
        }

        public override string Namespace
        {
            get { return BaseType.Namespace; }
        }

        public override IResolutionScope ResolutionScope
        {
            get { return BaseType.ResolutionScope; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte)
                   + BaseType.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteByte((byte)ElementType);
            BaseType.Write(context);
        }
    }
}
