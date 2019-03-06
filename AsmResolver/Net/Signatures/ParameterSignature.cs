using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents the signature of a single parameter defined in a method signature, specifying the type of the parameter.
    /// </summary>
    public class ParameterSignature : BlobSignature
    {
        /// <summary>
        /// Reads a single parameter signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the parameter resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static ParameterSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        /// <summary>
        /// Reads a single parameter signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the parameter resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static ParameterSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            return new ParameterSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public ParameterSignature(TypeSignature parameterType)
        {
            ParameterType = parameterType;
        }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        public TypeSignature ParameterType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return ParameterType.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            ParameterType.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            ParameterType.Write(buffer, writer);
        }
    }
}
