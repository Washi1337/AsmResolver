using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class PermissionSetSignature : BlobSignature
    {
        public static PermissionSetSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            var signature = new PermissionSetSignature();
            
            var signatureHeader = reader.ReadByte();
            if (signatureHeader != '.')
                throw new ArgumentException("Signature doesn't refer to a valid permission set signature.");

            uint attributeCount;
            if (!reader.TryReadCompressedUInt32(out attributeCount))
                return signature;

            for (int i = 0; i < attributeCount; i++)
                signature.Attributes.Add(SecurityAttributeSignature.FromReader(image, reader));
            return signature;
        }

        public PermissionSetSignature()
        {
            Attributes = new List<SecurityAttributeSignature>();
        }

        public IList<SecurityAttributeSignature> Attributes
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(sizeof (byte) +
                          Attributes.Count.GetCompressedSize() +
                          Attributes.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)'.');
            writer.WriteCompressedUInt32((uint)Attributes.Count);
            foreach (var attribute in Attributes)
                attribute.Write(buffer, writer);
        }
    }
}
