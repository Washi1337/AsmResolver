using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Signatures
{
    public class GenericInstanceMethodSignature : BlobSignature
    {
        public static GenericInstanceMethodSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            if (!reader.CanRead(sizeof(byte)) || reader.ReadByte() != 0x0A)
                throw new ArgumentException("Signature does not refer to a valid generic instance method signature.");

            var signature = new GenericInstanceMethodSignature();
            uint count;
            if (!reader.TryReadCompressedUInt32(out count))
                return signature;

            for (int i = 0; i < count; i++)
                signature.GenericArguments.Add(TypeSignature.FromReader(header, reader));

            return signature;
        }

        public GenericInstanceMethodSignature()
        {
            GenericArguments = new List<TypeSignature>();
        }

        public IList<TypeSignature> GenericArguments
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(sizeof (byte) +
                          GenericArguments.Count.GetCompressedSize() +
                          GenericArguments.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte(0x0A);
            writer.WriteCompressedUInt32((uint)GenericArguments.Count);
            foreach (var argument in GenericArguments)
                argument.Write(context);
        }
    }
}
