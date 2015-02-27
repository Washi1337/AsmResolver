using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class ByReferenceTypeSignature : TypeSignature
    {
        public new static ByReferenceTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new ByReferenceTypeSignature(TypeSignature.FromReader(header, reader));
        }

        public ByReferenceTypeSignature(TypeSignature baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException("baseType");
            BaseType = baseType;
        }

        public override ElementType ElementType
        {
            get { return ElementType.ByRef; }
        }

        public TypeSignature BaseType
        {
            get;
            set;
        }

        public override string Name
        {
            get { return BaseType.Name + '&'; }
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
            return 1 + BaseType.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteByte((byte)ElementType);
            BaseType.Write(context);
        }
    }
}
