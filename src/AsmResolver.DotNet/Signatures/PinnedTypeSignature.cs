using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
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
        public override string Name => BaseType.Name ?? NullTypeToString;

        /// <inheritdoc />
        public override bool IsValueType => BaseType.IsValueType;

        /// <inheritdoc />
        public override TypeSignature GetReducedType(RuntimeContext? context) => BaseType.GetReducedType(context);

        /// <inheritdoc />
        public override TypeSignature GetVerificationType(RuntimeContext? context) => BaseType.GetVerificationType(context);

        /// <inheritdoc />
        public override TypeSignature GetIntermediateType(RuntimeContext? context) => BaseType.GetIntermediateType(context);

        /// <inheritdoc />
        public override TypeSignature? GetDirectBaseClass(RuntimeContext? context) => BaseType.GetDirectBaseClass(context);

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
