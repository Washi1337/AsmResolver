using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents the type of an object, using a type definition or reference defined in one of the metadata tables.
    /// </summary>
    public class TypeDefOrRefSignature : TypeSignature, IResolvable
    {
        /// <summary>
        /// Reads a single type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static TypeDefOrRefSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }
        
        /// <summary>
        /// Reads a single type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static TypeDefOrRefSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            var type = ReadTypeDefOrRef(image, reader, protection);
            return type == null ? null : new TypeDefOrRefSignature(type);
        }

        public TypeDefOrRefSignature(ITypeDefOrRef type)
            : this(type, false)
        {
        }

        public TypeDefOrRefSignature(ITypeDefOrRef type, bool isValueType)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsValueType = isValueType;
        }

        public override ElementType ElementType => IsValueType ? ElementType.ValueType : ElementType.Class;

        /// <summary>
        /// Gets or sets the type definition or reference to use.   
        /// </summary>
        public ITypeDefOrRef Type
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Name => Type?.Name;

        /// <inheritdoc />
        public override string Namespace => Type?.Namespace;

        /// <inheritdoc />
        public override ITypeDescriptor DeclaringTypeDescriptor => Type?.DeclaringTypeDescriptor;

        /// <inheritdoc />
        public override IResolutionScope ResolutionScope => Type?.ResolutionScope;

        /// <inheritdoc />
        public override ITypeDescriptor GetElementType()
        {
            return Type.GetElementType();
        }

        /// <inheritdoc />
        public override ITypeDefOrRef ToTypeDefOrRef()
        {
            return Type;
        }

        public override TypeSignature InstantiateGenericTypes(IGenericContext context)
        {
            if (Type is TypeSpecification typeSpec)
                return typeSpec.Signature.InstantiateGenericTypes(context);
            return this;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            var encoder = buffer.TableStreamBuffer
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return sizeof(byte) +
                   encoder.EncodeToken(buffer.TableStreamBuffer.GetTypeToken(Type)).GetCompressedSize() +
                   base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            buffer.TableStreamBuffer.GetTypeToken(Type);
            buffer.TableStreamBuffer.GetResolutionScopeToken(ResolutionScope);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            WriteTypeDefOrRef(buffer, writer, Type);
            base.Write(buffer, writer);
        }

        /// <inheritdoc />
        public IMetadataMember Resolve()
        {
            return Type.Resolve();
        }
    }
}
