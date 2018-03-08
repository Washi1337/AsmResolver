using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class TypeDefOrRefSignature : TypeSignature, IResolvable
    {
        public new static TypeDefOrRefSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            var type = ReadTypeDefOrRef(image, reader);
            return type == null ? null : new TypeDefOrRefSignature(type);
        }

        public TypeDefOrRefSignature(ITypeDefOrRef type)
            : this(type, false)
        {
        }

        public TypeDefOrRefSignature(ITypeDefOrRef type, bool isValueType)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            Type = type;
            IsValueType = isValueType;
        }

        public override ElementType ElementType
        {
            get { return IsValueType ? ElementType.ValueType : ElementType.Class; }
        }

        public ITypeDefOrRef Type
        {
            get;
            set;
        }

        public override string Name
        {
            get { return Type.Name; }
        }

        public override string Namespace
        {
            get { return Type.Namespace; }
        }

        public override ITypeDescriptor DeclaringTypeDescriptor
        {
            get { return Type.DeclaringTypeDescriptor; }
        }

        public override IResolutionScope ResolutionScope
        {
            get { return Type.ResolutionScope; }
        }

        public override ITypeDescriptor GetElementType()
        {
            return Type.GetElementType();
        }

        public override uint GetPhysicalLength()
        {
            var encoder = Type.Image.Header.GetStream<TableStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return sizeof (byte) +
                   encoder.EncodeToken(Type.MetadataToken).GetCompressedSize();
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            WriteTypeDefOrRef(buffer, writer, Type);
        }

        public IMetadataMember Resolve()
        {
            return Type.Resolve();
        }
    }
}
