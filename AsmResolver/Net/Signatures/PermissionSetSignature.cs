using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Signatures
{
    public class PermissionSetSignature : BlobSignature
    {
        public static PermissionSetSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            var signature = new PermissionSetSignature()
            {
                StartOffset = reader.Position
            };

            var signatureHeader = reader.ReadByte();
            if (signatureHeader != '.')
                throw new ArgumentException("Signature doesn't refer to a valid permission set signature.");

            uint attributeCount;
            if (!reader.TryReadCompressedUInt32(out attributeCount))
                return signature;

            for (int i = 0; i < attributeCount; i++)
                signature.Attributes.Add(SecurityAttributeSignature.FromReader(header, reader));
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

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)'.');
            writer.WriteCompressedUInt32((uint)Attributes.Count);
            foreach (var attribute in Attributes)
                attribute.Write(context);
        }
    }
}
