using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class GenericInstanceTypeSignature : TypeSignature
    {
        public new static GenericInstanceTypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            var tableStream = header.GetStream<TableStream>();

            var elementType = (ElementType)reader.ReadByte();

            var type =
                (ITypeDefOrRef)
                    tableStream.ResolveMember(
                        tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(reader.ReadCompressedUInt32()));

            var signature = new GenericInstanceTypeSignature(type)
            {
                IsValueType = elementType == ElementType.ValueType
            };

            var count = reader.ReadCompressedUInt32();
            for (int i = 0; i < count; i++)
                signature.GenericArguments.Add(TypeSignature.FromReader(header, reader));

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
            get { return GenericType.Name + '<' + string.Join(", ", GenericArguments.Select(x => x.FullName)) + '>'; }
        }

        public override string Namespace
        {
            get { return GenericType.Namespace; }
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
                GenericType.Header.GetStream<TableStream>()
                    .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return (uint)(sizeof (byte) +
                          sizeof (byte) +
                          encoder.EncodeToken(GenericType.MetadataToken).GetCompressedSize() +
                          GenericArguments.Count.GetCompressedSize() +
                          GenericArguments.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)ElementType);
            writer.WriteByte((byte)(IsValueType ? ElementType.ValueType : ElementType.Class));

            var encoder =
                context.Assembly.NetDirectory.MetadataHeader.GetStream<TableStream>()
                    .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            writer.WriteCompressedUInt32(encoder.EncodeToken(GenericType.MetadataToken));

            writer.WriteCompressedUInt32((uint)GenericArguments.Count);
            foreach (var argument in GenericArguments)
                argument.Write(context);
        }
    }
}
