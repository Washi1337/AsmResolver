using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public sealed class MsCorLibTypeSignature : TypeSignature
    {
        private readonly ElementType _elementType;

        internal MsCorLibTypeSignature(ITypeDefOrRef type, ElementType elementType, bool isValueType)
        {
            Type = type;
            _elementType = elementType;
            IsValueType = isValueType;
        }

        public ITypeDefOrRef Type
        {
            get;
            private set;
        }

        public override ElementType ElementType
        {
            get { return _elementType; }
        }

        public override string Name
        {
            get { return Type.Name; }
        }

        public override string Namespace
        {
            get { return Type.Namespace; }
        }

        public override IResolutionScope ResolutionScope
        {
            get { return Type.ResolutionScope; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte);
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteByte((byte)ElementType);
        }
    }
}
