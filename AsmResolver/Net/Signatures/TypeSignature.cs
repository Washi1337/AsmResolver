
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public abstract class TypeSignature : ExtendableBlobSignature, ITypeDescriptor
    {
        public static TypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader, bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }

        public static TypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader, bool readToEnd, RecursionProtection protection)
        {
            var signature = ReadTypeSignature(image, reader, protection);
            if (readToEnd)
                signature.ExtraData = reader.ReadToEnd();
            return signature;
        }

        private static TypeSignature ReadTypeSignature(
            MetadataImage image,
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            var elementType = (ElementType) reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Array:
                    return ArrayTypeSignature.FromReader(image, reader, protection);
                case ElementType.Boxed:
                    return BoxedTypeSignature.FromReader(image, reader, protection);
                case ElementType.ByRef:
                    return ByReferenceTypeSignature.FromReader(image, reader, protection);
                case ElementType.CModOpt:
                    return OptionalModifierSignature.FromReader(image, reader, protection);
                case ElementType.CModReqD:
                    return RequiredModifierSignature.FromReader(image, reader, protection);
                case ElementType.Class:
                    return TypeDefOrRefSignature.FromReader(image, reader, protection);
                case ElementType.FnPtr:
                    return FunctionPointerTypeSignature.FromReader(image, reader, protection);
                case ElementType.GenericInst:
                    return GenericInstanceTypeSignature.FromReader(image, reader, protection);
                case ElementType.MVar:
                    return GenericParameterSignature.FromReader(image, reader, GenericParameterType.Method);
                case ElementType.Pinned:
                    return PinnedTypeSignature.FromReader(image, reader, protection);
                case ElementType.Ptr:
                    return PointerTypeSignature.FromReader(image, reader, protection);
                case ElementType.Sentinel:
                    return SentinelTypeSignature.FromReader(image, reader, protection);
                case ElementType.SzArray:
                    return SzArrayTypeSignature.FromReader(image, reader, protection);
                case ElementType.ValueType:
                    var type = TypeDefOrRefSignature.FromReader(image, reader, protection);
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

        protected static ITypeDefOrRef ReadTypeDefOrRef(MetadataImage image, IBinaryStreamReader reader, RecursionProtection protection)
        {
            var tableStream = image.Header.GetStream<TableStream>();

            if (!reader.TryReadCompressedUInt32(out uint codedIndex))
                return null;

            var token = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(codedIndex);
            if (protection.TraversedTokens.Add(token))
            {
                image.TryResolveMember(token, out var type);
                return type as ITypeDefOrRef;
            }

            return null;
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

        public virtual string FullName =>
            DeclaringTypeDescriptor != null
                ? DeclaringTypeDescriptor.FullName + "+" + Name
                : (string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name);

        public virtual ITypeDescriptor GetElementType()
        {
            return this;
        }

        /// <inheritdoc />
        public virtual ITypeDefOrRef ToTypeDefOrRef()
        {
            return new TypeSpecification(this);
        }

        TypeSignature ITypeDescriptor.ToTypeSignature()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
