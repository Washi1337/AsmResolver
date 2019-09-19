using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents the type of an object that stores the memory address of another object.
    /// </summary>
    public class PointerTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Reads a single pointer type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static PointerTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        /// <summary>
        /// Reads a single pointer type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static PointerTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new PointerTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public PointerTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Ptr;

        /// <inheritdoc />
        public override string Name => BaseType.Name + "*";

        public override TypeSignature InstantiateGenericTypes(IGenericContext context)
        {
            return new PointerTypeSignature(BaseType.InstantiateGenericTypes(context));
        }
    }
}
