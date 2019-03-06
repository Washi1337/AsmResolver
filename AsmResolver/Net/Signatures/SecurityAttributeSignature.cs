using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a single security attribute that restricts the ways a member can be used. 
    /// </summary>
    public class SecurityAttributeSignature : ExtendableBlobSignature
    {
        /// <summary>
        /// Reads a single security attribute at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read attribute.</returns>
        public static SecurityAttributeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            var signature = new SecurityAttributeSignature
            {
                TypeName = reader.ReadSerString(),
            };

            reader.ReadCompressedUInt32();

            if (!reader.TryReadCompressedUInt32(out uint argumentCount))
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

        /// <summary>
        /// Gets or sets the name of the security attribute referenced.
        /// </summary>
        public string TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of named arguments that assign a value to a field or a property defined in the attribute. 
        /// </summary>
        public IList<CustomAttributeNamedArgument> NamedArguments
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            uint argumentsSize = (uint)NamedArguments.Sum(x => x.GetPhysicalLength(buffer));
            return TypeName.GetSerStringSize() +
                   (NamedArguments.Count == 0
                       ? 2 * sizeof(byte)
                       : NamedArguments.Count.GetCompressedSize() +
                         argumentsSize.GetCompressedSize() +
                         argumentsSize)
                   + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            foreach (var argument in NamedArguments)
                argument.Prepare(buffer);
        }

        /// <inheritdoc />
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
                    (uint)(NamedArguments.Count.GetCompressedSize() + NamedArguments.Sum(x => x.GetPhysicalLength(buffer))));
                writer.WriteCompressedUInt32((uint)NamedArguments.Count);
                foreach (var argument in NamedArguments)
                {
                    argument.Write(buffer, writer);
                }
            }

            base.Write(buffer, writer);
        }
    }
}