using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents the signature of a generic instance type. That is, a type reference that instantiates a generic
    /// type with type arguments.
    /// </summary>
    public class GenericInstanceTypeSignature : TypeSignature, IGenericArgumentsProvider
    {
        /// <summary>
        /// Reads a single generic instance type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static GenericInstanceTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        /// <summary>
        /// Reads a single generic instance type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static GenericInstanceTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            if (!reader.CanRead(sizeof (byte)))
                return null;

            var elementType = (ElementType) reader.ReadByte();
            var type = ReadTypeDefOrRef(image, reader, protection);

            var signature = new GenericInstanceTypeSignature(type)
            {
                IsValueType = elementType == ElementType.ValueType
            };

            if (!reader.TryReadCompressedUInt32(out uint count))
                return signature;

            for (int i = 0; i < count; i++)
                signature.GenericArguments.Add(TypeSignature.FromReader(image, reader, false, protection));

            return signature;
        }

        public GenericInstanceTypeSignature(ITypeDefOrRef genericType)
        {
            GenericType = genericType ?? throw new ArgumentNullException(nameof(genericType));
            GenericArguments = new List<TypeSignature>();
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.GenericInst;

        /// <summary>
        /// Gets or sets the generic type that is instantiated. 
        /// </summary>
        public ITypeDefOrRef GenericType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of type arguments used to instantiate the generic type.
        /// </summary>
        public IList<TypeSignature> GenericArguments
        {
            get;
        }

        /// <inheritdoc />
        public override string Name => GenericType?.Name;

        /// <inheritdoc />
        public override string Namespace => GenericType?.Namespace;

        /// <inheritdoc />
        public override string FullName =>
            base.FullName + '<' + string.Join(", ", GenericArguments.Select(x => x.FullName)) + '>';

        /// <inheritdoc />
        public override IResolutionScope ResolutionScope => GenericType?.ResolutionScope;

        /// <inheritdoc />
        public override ITypeDescriptor GetElementType()
        {
            return GenericType.GetElementType();
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            var encoder =
                GenericType.Image.Header.GetStream<TableStream>()
                    .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return (uint) (sizeof(byte) +
                           sizeof(byte) +
                           encoder.EncodeToken(buffer.TableStreamBuffer.GetTypeToken(GenericType)).GetCompressedSize() +
                           GenericArguments.Count.GetCompressedSize() +
                           GenericArguments.Sum(x => x.GetPhysicalLength(buffer)))
                   + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            buffer.TableStreamBuffer.GetTypeToken(GenericType);
            foreach (var argument in GenericArguments)
                argument.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            writer.WriteByte((byte)(IsValueType ? ElementType.ValueType : ElementType.Class));

            WriteTypeDefOrRef(buffer, writer, GenericType);

            writer.WriteCompressedUInt32((uint)GenericArguments.Count);
            foreach (var argument in GenericArguments)
                argument.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
