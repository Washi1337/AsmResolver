using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents the type of a single-dimension array with a lower bound of zero.
    /// </summary>
    /// <remarks>
    /// This signature can also be used to construct jagged arrays such as <c>int[][][]</c> by nesting multiple instances
    /// of this class. For more complex array type constructions, such as multi dimensional types, use <see cref="ArrayTypeSignature"/>.
    /// </remarks>
    public class SzArrayTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Reads a single array type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static SzArrayTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        /// <summary>
        /// Reads a single array type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static SzArrayTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new SzArrayTypeSignature(TypeSignature.FromReader(image, reader, false, protection));
        }

        public SzArrayTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.SzArray;

        /// <inheritdoc />
        public override string Name => BaseType.Name + "[]";
    }
}
