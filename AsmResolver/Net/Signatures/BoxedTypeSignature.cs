using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a type signature of a boxed object.
    /// </summary>
    public class BoxedTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Reads a single boxed type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature is defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static BoxedTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }
        
        /// <summary>
        /// Reads a single boxed type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature is defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static BoxedTypeSignature FromReader(MetadataImage image, 
            IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            return new BoxedTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public BoxedTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Boxed;
    }
}
