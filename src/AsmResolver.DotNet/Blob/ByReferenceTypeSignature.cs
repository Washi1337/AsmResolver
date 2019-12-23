using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Blob
{
    /// <summary>
    /// Represents a type signature that describes a type that is passed on by reference.
    /// </summary>
    public class ByReferenceTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Creates a new by reference type signature.
        /// </summary>
        /// <param name="baseType">The type that is passed on by reference.</param>
        public ByReferenceTypeSignature(TypeSignature baseType) 
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.ByRef;

        /// <inheritdoc />
        public override string Name => BaseType.Name + '&';

        /// <inheritdoc />
        public override bool IsValueType => false;
    }
}