
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public abstract class TypeSignature : BlobSignature, ITypeDescriptor
    {
        public static TypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return ReadTypeSignature(image, reader);
        }

        private static TypeSignature ReadTypeSignature(MetadataImage image, IBinaryStreamReader reader)
        {
            var elementType = (ElementType) reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Array:
                    return ArrayTypeSignature.FromReader(image, reader);
                case ElementType.Boxed:
                    return BoxedTypeSignature.FromReader(image, reader);
                case ElementType.ByRef:
                    return ByReferenceTypeSignature.FromReader(image, reader);
                case ElementType.CModOpt:
                    return OptionalModifierSignature.FromReader(image, reader);
                case ElementType.CModReqD:
                    return RequiredModifierSignature.FromReader(image, reader);
                case ElementType.Class:
                    return TypeDefOrRefSignature.FromReader(image, reader);
                case ElementType.FnPtr:
                    return FunctionPointerTypeSignature.FromReader(image, reader);
                case ElementType.GenericInst:
                    return GenericInstanceTypeSignature.FromReader(image, reader);
                case ElementType.MVar:
                    return GenericParameterSignature.FromReader(image, reader, GenericParameterType.Method);
                case ElementType.Pinned:
                    return PinnedTypeSignature.FromReader(image, reader);
                case ElementType.Ptr:
                    return PointerTypeSignature.FromReader(image, reader);
                case ElementType.Sentinel:
                    return SentinelTypeSignature.FromReader(image, reader);
                case ElementType.SzArray:
                    return SzArrayTypeSignature.FromReader(image, reader);
                case ElementType.ValueType:
                    var type = TypeDefOrRefSignature.FromReader(image, reader);
                    type.IsValueType = true;
                    return type;
                case ElementType.Var:
                    return GenericParameterSignature.FromReader(image, reader, GenericParameterType.Type);
                default:
                    return MsCorLibTypeSignature.FromElementType(image, elementType);
            }
        }

        public static TypeSignature FromAssemblyQualifiedName(MetadataImage image, string assemblyQualifiedName)
        {
            return TypeNameParser.ParseType(image, assemblyQualifiedName);
        }

        public static TypeSignature ReadFieldOrPropType(MetadataImage image, IBinaryStreamReader reader)
        {
            var elementType = (ElementType)reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Boxed:
                    return image.TypeSystem.Object;
                case ElementType.SzArray:
                    return new SzArrayTypeSignature(ReadFieldOrPropType(image, reader));
                case ElementType.Enum:
                    return FromAssemblyQualifiedName(image, reader.ReadSerString());
                default:
                    return MsCorLibTypeSignature.FromElementType(image, elementType);
            }
        }

        protected static ITypeDefOrRef ReadTypeDefOrRef(MetadataImage image, IBinaryStreamReader reader)
        {
            var tableStream = image.Header.GetStream<TableStream>();

            uint codedIndex;
            if (!reader.TryReadCompressedUInt32(out codedIndex))
                return null;

            IMetadataMember type;
            image.TryResolveMember(tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(codedIndex), out type);

            return type as ITypeDefOrRef;
        }

        protected static void WriteTypeDefOrRef(MetadataBuffer buffer, IBinaryStreamWriter writer, ITypeDefOrRef type)
        {
            var encoder = buffer.TableStreamBuffer.GetIndexEncoder(CodedIndex.TypeDefOrRef);
            writer.WriteCompressedUInt32(encoder.EncodeToken(buffer.TableStreamBuffer.GetTypeToken(type)));
        }

        public abstract ElementType ElementType
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract string Namespace
        {
            get;
        }

        public virtual ITypeDescriptor DeclaringTypeDescriptor
        {
            get { return ResolutionScope as ITypeDescriptor; }
        }

        public abstract IResolutionScope ResolutionScope
        {
            get;
        }

        public virtual bool IsValueType
        {
            get;
            set;
        }

        public virtual string FullName
        {
            get {
                return DeclaringTypeDescriptor != null
                    ? DeclaringTypeDescriptor.FullName + "+" + Name
                    : (string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name);
            }
        }

        public virtual ITypeDescriptor GetElementType()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
