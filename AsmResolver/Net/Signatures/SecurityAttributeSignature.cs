using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class SecurityAttributeSignature : BlobSignature
    {
        public static SecurityAttributeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            var signature = new SecurityAttributeSignature
            {
                TypeName = reader.ReadSerString(),
            };

            reader.ReadCompressedUInt32(); 

            uint argumentCount;
            if (!reader.TryReadCompressedUInt32(out argumentCount))
                return signature;

            if (argumentCount == 0)
                return signature;

            for (int i = 0; i < argumentCount; i++)
                signature.NamedArguments.Add(CustomAttributeNamedArgument.FromReader(image, reader));
            
            return signature;
        }

        public SecurityAttributeSignature()
        {
            NamedArguments = new List<CustomAttributeNamedArgument>();
        }

        public string TypeName
        {
            get;
            set;
        }

        public IList<CustomAttributeNamedArgument> NamedArguments
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            uint argumentsSize = (uint)NamedArguments.Sum(x => x.GetPhysicalLength());
            return (uint)(TypeName.GetSerStringSize() +
                          (NamedArguments.Count == 0
                              ? 2 * sizeof (byte)
                              : NamedArguments.Count.GetCompressedSize() +
                                argumentsSize.GetCompressedSize() +
                                argumentsSize));
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteSerString(TypeName);

            if (NamedArguments.Count == 0)
            {
                writer.WriteCompressedUInt32(1);
                writer.WriteCompressedUInt32(0);
            }
            else
            {
                writer.WriteCompressedUInt32(
                    (uint)(NamedArguments.Count.GetCompressedSize() + NamedArguments.Sum(x => x.GetPhysicalLength())));
                writer.WriteCompressedUInt32((uint)NamedArguments.Count);
                foreach (var argument in NamedArguments)
                {
                    argument.Write(buffer, writer);
                }
            }
        }
    }
}