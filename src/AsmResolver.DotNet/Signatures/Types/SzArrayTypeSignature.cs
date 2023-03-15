using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type signature describing a single dimension array with 0 as a lower bound.
    /// </summary>
    public class SzArrayTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Creates a new single-dimension array signature with 0 as a lower bound.
        /// </summary>
        /// <param name="baseType">The type of the elements to store in the array.</param>
        public SzArrayTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.SzArray;

        /// <inheritdoc />
        public override string Name => $"{BaseType.Name ?? NullTypeToString}[]";

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <inheritdoc />
        public override TypeSignature? GetDirectBaseClass() => Module?.CorLibTypeFactory.CorLibScope
            .CreateTypeReference("System", "Array")
            .ToTypeSignature(false)
            .ImportWith(Module.DefaultImporter);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) =>
            visitor.VisitSzArrayType(this);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor,
            TState state) =>
            visitor.VisitSzArrayType(this, state);
    }
}
