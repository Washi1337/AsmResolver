using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public abstract class TypeSpecificationSignature : TypeSignature
    {
        protected TypeSpecificationSignature()
        {
        }

        protected TypeSpecificationSignature(TypeSignature baseType)
        {
            BaseType = baseType;
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
            return sizeof(byte) +
                   BaseType.GetPhysicalLength();
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            BaseType.Write(buffer, writer);
        }
    }
}
