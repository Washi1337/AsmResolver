using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class GenericInstanceTypeSignature : TypeSignature, IGenericArgumentsProvider
    {
        public static GenericInstanceTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
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

        public override ElementType ElementType => ElementType.GenericInst;

        public ITypeDefOrRef GenericType
        {
            get;
            set;
        }

        public IList<TypeSignature> GenericArguments
        {
            get;
        }

        public override string Name => GenericType.Name;

        public override string Namespace => GenericType.Namespace;

        public override string FullName =>
            base.FullName + '<' + string.Join(", ", GenericArguments.Select(x => x.FullName)) + '>';

        public override IResolutionScope ResolutionScope => GenericType.ResolutionScope;

        public override ITypeDescriptor GetElementType()
        {
            return GenericType.GetElementType();
        }

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

        public override void Prepare(MetadataBuffer buffer)
        {
            buffer.TableStreamBuffer.GetTypeToken(GenericType);
            foreach (var argument in GenericArguments)
                argument.Prepare(buffer);
        }

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
