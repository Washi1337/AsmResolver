using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class PinnedTypeSignature : TypeSignature
    {
        public new static PinnedTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new PinnedTypeSignature(TypeSignature.FromReader(header, reader));
        }

        public PinnedTypeSignature(TypeSignature baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException("baseType");
            BaseType = baseType;
        }

        public override ElementType ElementType
        {
            get { return ElementType.Pinned; }
        }

        public TypeSignature BaseType
        {
            get;
            set;
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

        public override ITypeDescriptor GetElementType()
        {
            return BaseType.GetElementType();
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
