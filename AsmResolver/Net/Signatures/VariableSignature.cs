using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a single variable defined in a CIL method body.
    /// </summary>
    public class VariableSignature : ExtendableBlobSignature
    {
        /// <summary>
        /// Reads a single variable signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static VariableSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        /// <summary>
        /// Reads a single variable signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static VariableSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new VariableSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public VariableSignature(TypeSignature variableType)
        {
            VariableType = variableType;
        }

        /// <summary>
        /// Gets or sets the type of the variable.
        /// </summary>
        public TypeSignature VariableType
        {
            get;
            set;
        }

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return VariableType.GetPhysicalLength(buffer)
                + base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            VariableType.Prepare(buffer);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            VariableType.Write(buffer, writer);
            base.Write(buffer, writer);
        }
    }
}
