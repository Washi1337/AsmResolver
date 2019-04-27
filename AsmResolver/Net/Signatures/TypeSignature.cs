
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Provides a base for all type signatures stored in the blob stream, representing a type of an object.
    /// </summary>
    public abstract class TypeSignature : ExtendableBlobSignature, ITypeDescriptor
    {
        /// <summary>
        /// Reads a single type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <returns>The read signature.</returns>
        public static TypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader, bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }

        /// <summary>
        /// Reads a single type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static TypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader, bool readToEnd, RecursionProtection protection)
        {
            var signature = ReadTypeSignature(image, reader, protection);
            if (signature != null && readToEnd)
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

        /// <summary>
        /// Translates a fully qualified name of a type to a type signature.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="assemblyQualifiedName">The fully qualified name.</param>
        /// <returns>The type signature.</returns>
        public static TypeSignature FromAssemblyQualifiedName(MetadataImage image, string assemblyQualifiedName)
        {
            return TypeNameParser.ParseType(image, assemblyQualifiedName);
        }

        /// <summary>
        /// Reads a single field or property type.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The type signature.</returns>
        public static TypeSignature ReadFieldOrPropType(MetadataImage image, IBinaryStreamReader reader)
        {
            var elementType = (ElementType) reader.ReadByte();
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

        /// <summary>
        /// Reads a single coded index to a type definition or reference and resolves it.
        /// </summary>
        /// <param name="image">The image the type resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The type, or <c>null</c> if recursion was detected.</returns>
        protected static ITypeDefOrRef ReadTypeDefOrRef(MetadataImage image, IBinaryStreamReader reader, RecursionProtection protection)
        {
            var tableStream = image.Header.GetStream<TableStream>();

            if (!reader.TryReadCompressedUInt32(out uint codedIndex))
                return null;

            // If the resolved type is a TypeSpec, it can be a (malicious) loop to the same blob signature that we
            // were coming from. 
            var token = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(codedIndex);
            if (token.TokenType == MetadataTokenType.TypeSpec && !protection.TraversedTokens.Add(token))
                return InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.MetadataLoop);
            
            image.TryResolveMember(token, out var type);
            return type as ITypeDefOrRef;
        }

        /// <summary>
        /// Write a coded index to a type to the output stream.
        /// </summary>
        /// <param name="buffer">The metadata buffer to add the type to.</param>
        /// <param name="writer">The writer to use.</param>
        /// <param name="type">The type to write.</param>
        protected static void WriteTypeDefOrRef(MetadataBuffer buffer, IBinaryStreamWriter writer, ITypeDefOrRef type)
        {
            var encoder = buffer.TableStreamBuffer.GetIndexEncoder(CodedIndex.TypeDefOrRef);
            writer.WriteCompressedUInt32(encoder.EncodeToken(buffer.TableStreamBuffer.GetTypeToken(type)));
        }

        /// <summary>
        /// Gets the raw element type of the type signature. This is the first byte of every type signature.
        /// </summary>
        public abstract ElementType ElementType
        {
            get;
        }

        /// <inheritdoc />
        public abstract string Name
        {
            get;
        }

        /// <inheritdoc />
        public abstract string Namespace
        {
            get;
        }

        /// <inheritdoc />
        public virtual ITypeDescriptor DeclaringTypeDescriptor => ResolutionScope as ITypeDescriptor;

        /// <inheritdoc />
        public abstract IResolutionScope ResolutionScope
        {
            get;
        }

        /// <inheritdoc />
        public virtual bool IsValueType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public virtual string FullName =>
            DeclaringTypeDescriptor != null
                ? DeclaringTypeDescriptor.FullName + "+" + Name
                : (string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name);

        /// <inheritdoc />
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

        public abstract TypeSignature InstantiateGenericTypes(IGenericContext context);

        public override string ToString()
        {
            return FullName;
        }
    }
}
