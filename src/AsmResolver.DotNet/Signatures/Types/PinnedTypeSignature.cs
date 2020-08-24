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
        public override string Name => BaseType?.Name ?? NullTypeToString;

        /// <inheritdoc />
        public override bool IsValueType => BaseType.IsValueType;
        
        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitPinnedType(this);
    }
}