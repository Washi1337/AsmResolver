using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Describes a set of security-related permissions associated to a member. 
    /// </summary>
    public class PermissionSetSignature : ExtendableBlobSignature
    {
        /// <summary>
        /// Reads a set of permissions at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the permission set resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The permission set.</returns>
        /// <exception cref="ArgumentException">Occurs when the reader does not point to a valid set of permissions.</exception>
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

        /// <summary>
        /// Gets a collection of attributes defined in the permission set.
        /// </summary>
        public IList<SecurityAttributeSignature> Attributes
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint) (sizeof(byte) +
                           Attributes.Count.GetCompressedSize() +
                           Attributes.Sum(x => x.GetPhysicalLength(buffer))) +
                   base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            foreach (var attribute in Attributes)
                attribute.Prepare(buffer);
        }

        /// <inheritdoc />
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
