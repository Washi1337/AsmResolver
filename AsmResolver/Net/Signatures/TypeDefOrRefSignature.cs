using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class TypeDefOrRefSignature : TypeSignature
    {
        public new static TypeDefOrRefSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            var type = ReadTypeDefOrRef(header, reader);
            return type == null ? null : new TypeDefOrRefSignature(type);
        }

        public TypeDefOrRefSignature(ITypeDefOrRef type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            Type = type;
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
            var encoder = Type.Header.GetStream<TableStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return sizeof (byte) +
                   encoder.EncodeToken(Type.MetadataToken).GetCompressedSize();
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)ElementType);

            WriteTypeDefOrRef(context.Assembly.NetDirectory.MetadataHeader, context.Writer, Type);

        }
    }
}
