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
        public new static GenericInstanceTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            if (!reader.CanRead(sizeof (byte)))
                return null;

            long position = reader.Position;

            var elementType = (ElementType)reader.ReadByte();
            
            var type = ReadTypeDefOrRef(image, reader);

            var signature = new GenericInstanceTypeSignature(type)
            {
                IsValueType = elementType == ElementType.ValueType
            };

            uint count;
            if (!reader.TryReadCompressedUInt32(out count))
                return signature;

            for (int i = 0; i < count; i++)
                signature.GenericArguments.Add(TypeSignature.FromReader(image, reader));

            return signature;
        }

        public GenericInstanceTypeSignature(ITypeDefOrRef genericType)
        {
            if (genericType == null)
                throw new ArgumentNullException("genericType");
            GenericType = genericType;
            GenericArguments = new List<TypeSignature>();
        }

        public override ElementType ElementType
        {
            get { return ElementType.GenericInst; }
        }

        public ITypeDefOrRef GenericType
        {
            get;
            set;
        }

        public IList<TypeSignature> GenericArguments
        {
            get;
            private set;
        }

        public override string Name
        {
            get { return GenericType.Name; }
        }

        public override string Namespace
        {
            get { return GenericType.Namespace; }
        }

        public override string FullName
        {
            get
            {
                return base.FullName +'<' + string.Join(", ", GenericArguments.Select(x => x.FullName)) + '>';
            }
        }

        public override IResolutionScope ResolutionScope
        {
            get { return GenericType.ResolutionScope; }
        }

        public override ITypeDescriptor GetElementType()
        {
            return GenericType.GetElementType();
        }

        public override uint GetPhysicalLength()
        {
            var encoder =
                GenericType.Image.Header.GetStream<TableStream>()
                    .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return (uint)(sizeof (byte) +
                          sizeof (byte) +
                          encoder.EncodeToken(GenericType.MetadataToken).GetCompressedSize() +
                          GenericArguments.Count.GetCompressedSize() +
                          GenericArguments.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            writer.WriteByte((byte)(IsValueType ? ElementType.ValueType : ElementType.Class));

            WriteTypeDefOrRef(buffer, writer, GenericType);

            writer.WriteCompressedUInt32((uint)GenericArguments.Count);
            foreach (var argument in GenericArguments)
                argument.Write(buffer, writer);
        }
    }
}
