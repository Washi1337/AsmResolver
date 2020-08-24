using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures.Security
{
    /// <summary>
    /// Represents a blob signature containing a set of security attributes. 
    /// </summary>
    public class PermissionSetSignature : ExtendableBlobSignature
    {
        /// <summary>
        /// Reads a permission set signature from the provided input blob stream.
        /// </summary>
        /// <param name="parentModule">The module the permission set resides in.</param>
        /// <param name="reader">The input blob stream.</param>
        /// <returns>The permission set.</returns>
        public static PermissionSetSignature FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader)
        {
            var result = new PermissionSetSignature();
            if (reader.ReadByte() != '.')
                return result;
            
            if (!reader.TryReadCompressedUInt32(out uint count))
                return result;

            for (int i = 0; i < count && reader.CanRead(1); i++)
                result.Attributes.Add(SecurityAttribute.FromReader(parentModule, reader));

            return result;
        }

        /// <summary>
        /// Gets the security attributes stored in the signature.
        /// </summary>
        public IList<SecurityAttribute> Attributes
        {
            get;
        } = new List<SecurityAttribute>();

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            
            writer.WriteByte((byte) '.');
            writer.WriteCompressedUInt32((uint) Attributes.Count);
            
            for (int i = 0; i < Attributes.Count; i++)
                Attributes[i].Write(context);
        }
    }
}