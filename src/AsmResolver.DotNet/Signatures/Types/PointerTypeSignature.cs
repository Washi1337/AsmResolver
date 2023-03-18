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
        public override string? Name => $"{BaseType?.Name ?? NullTypeToString}*";

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <inheritdoc />
        protected override bool IsDirectlyCompatibleWith(TypeSignature other, SignatureComparer comparer)
        {
            if (base.IsDirectlyCompatibleWith(other, comparer))
                return true;

            if (other is not PointerTypeSignature otherPointer)
                return false;

            var v = BaseType.GetVerificationType();
            var w = otherPointer.BaseType.GetVerificationType();
            return comparer.Equals(v, w);
        }

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) =>
            visitor.VisitPointerType(this);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor,
            TState state) =>
            visitor.VisitPointerType(this, state);
    }
}
