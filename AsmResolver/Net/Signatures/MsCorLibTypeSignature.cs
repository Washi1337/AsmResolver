using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public sealed class MsCorLibTypeSignature : TypeSignature, IResolvable
    {
        public static MsCorLibTypeSignature FromElementType(MetadataImage image, ElementType elementType)
        {
            var type = image.TypeSystem.GetMscorlibType(elementType);
            if (type == null)
                throw new ArgumentException("Element type " + elementType + " is not recognized as a valid corlib type signature.");
            return type;
        }

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

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
        }

        public IMetadataMember Resolve()
        {
            return Type.Resolve();
        }
    }
}
