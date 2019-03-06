using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class PermissionSetSignature : ExtendableBlobSignature
    {
        public static PermissionSetSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            var signature = new PermissionSetSignature();
            
            byte signatureHeader = reader.ReadByte();
            if (signatureHeader != '.')
                throw new ArgumentException("Signature doesn't refer to a valid permission set signature.");

            if (!reader.TryReadCompressedUInt32(out uint attributeCount))
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

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (sizeof(byte) +
                           Attributes.Count.GetCompressedSize() +
                           Attributes.Sum(x => x.GetPhysicalLength(buffer))) +
                   base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            foreach (var attribute in Attributes)
                attribute.Prepare(buffer);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)'.');
            writer.WriteCompressedUInt32((uint)Attributes.Count);
            foreach (var attribute in Attributes)
                attribute.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
