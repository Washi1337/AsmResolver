using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class TypeDefOrRefSignature : TypeSignature, IResolvable
    {
        public static TypeDefOrRefSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
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

        public override string Name => Type.Name;

        public override string Namespace => Type.Namespace;

        public override ITypeDescriptor DeclaringTypeDescriptor => Type.DeclaringTypeDescriptor;

        public override IResolutionScope ResolutionScope => Type.ResolutionScope;

        public override ITypeDescriptor GetElementType()
        {
            return Type.GetElementType();
        }

        public override ITypeDefOrRef ToTypeDefOrRef()
        {
            return Type;
        }

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            var encoder = buffer.TableStreamBuffer
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return sizeof(byte) +
                   encoder.EncodeToken(buffer.TableStreamBuffer.GetTypeToken(Type)).GetCompressedSize() +
                   base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            buffer.TableStreamBuffer.GetTypeToken(Type);
            buffer.TableStreamBuffer.GetResolutionScopeToken(ResolutionScope);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            WriteTypeDefOrRef(buffer, writer, Type);
            base.Write(buffer, writer);
        }

        public IMetadataMember Resolve()
        {
            return Type.Resolve();
        }
    }
}
