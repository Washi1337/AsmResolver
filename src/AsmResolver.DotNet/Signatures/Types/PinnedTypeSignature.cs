using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type modifier indicating the value is pinned into memory, and the garbage collector cannot
    /// change the location of a value at runtime.
    /// </summary>
    public class PinnedTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Creates a new pinned type signature.
        /// </summary>
        /// <param name="baseType">The type to pin.</param>
        public PinnedTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Pinned;

        /// <inheritdoc />
        public override string? Name => BaseType.Name ?? NullTypeToString;

        /// <inheritdoc />
        public override bool IsValueType => BaseType.IsValueType;

        /// <inheritdoc />
        public override TypeSignature GetReducedType() => BaseType.GetReducedType();

        /// <inheritdoc />
        public override TypeSignature GetVerificationType() => BaseType.GetVerificationType();

        /// <inheritdoc />
        public override TypeSignature GetIntermediateType() => BaseType.GetIntermediateType();

        /// <inheritdoc />
        public override TypeSignature? GetDirectBaseClass() => BaseType.GetDirectBaseClass();

        /// <inheritdoc />
        public override TypeSignature StripModifiers() => BaseType.StripModifiers();

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) =>
            visitor.VisitPinnedType(this);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor,
            TState state) =>
            visitor.VisitPinnedType(this, state);

    }
}
