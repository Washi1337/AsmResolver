using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class GenericInstanceMethodSignature : CallingConventionSignature, IGenericArgumentsProvider
    {
        public new static GenericInstanceMethodSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            var signature = new GenericInstanceMethodSignature
            {
                Attributes = (CallingConventionAttributes)reader.ReadByte()
            };

            uint count;
            if (!reader.TryReadCompressedUInt32(out count))
                return signature;

            for (int i = 0; i < count; i++)
                signature.GenericArguments.Add(TypeSignature.FromReader(image, reader));

            return signature;
        }

        public GenericInstanceMethodSignature()
            : this(Enumerable.Empty<TypeSignature>())
        {
        }

        public GenericInstanceMethodSignature(params TypeSignature[] arguments)
            : this(arguments.AsEnumerable())
        {
        }

        public GenericInstanceMethodSignature(IEnumerable<TypeSignature> arguments)
        {
            GenericArguments = new List<TypeSignature>(arguments);
            Attributes = CallingConventionAttributes.GenericInstance;
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

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte(0x0A);
            writer.WriteCompressedUInt32((uint)GenericArguments.Count);
            foreach (var argument in GenericArguments)
                argument.Write(buffer, writer);
        }
    }
}
