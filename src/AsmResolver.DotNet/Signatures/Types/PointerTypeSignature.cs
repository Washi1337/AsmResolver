using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type signature that describes an unmanaged pointer that addresses a chunk of data in memory. 
    /// </summary>
    public class PointerTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Creates a new pointer type signature.
        /// </summary>
        /// <param name="baseType">The type of values the pointer addresses.</param>
        public PointerTypeSignature(TypeSignature baseType) 
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Ptr;

        /// <inheritdoc />
        public override string Name => $"{BaseType?.Name ?? NullTypeToString}*";

        /// <inheritdoc />
        public override bool IsValueType => false;
        
        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitPointerType(this);
    }
}