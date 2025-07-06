using System;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
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
        public override string Name => $"{BaseType.Name ?? NullTypeToString}&";

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <inheritdoc />
        public override TypeSignature GetVerificationType()
        {
            if (ContextModule is null)
                throw new InvalidOperationException("Cannot determine verification type of a non-imported type.");

            var factory = ContextModule.CorLibTypeFactory;
            return BaseType.GetReducedType().ElementType switch
            {
                ElementType.I1 or ElementType.Boolean => factory.SByte.MakeByReferenceType(),
                ElementType.I2 or ElementType.Char => factory.Int16.MakeByReferenceType(),
                ElementType.I4 => factory.Int32.MakeByReferenceType(),
                ElementType.I8 => factory.Int64.MakeByReferenceType(),
                ElementType.I => factory.IntPtr.MakeByReferenceType(),
                _ => base.GetVerificationType()
            };
        }

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) =>
            visitor.VisitByReferenceType(this);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor,
            TState state) =>
            visitor.VisitByReferenceType(this, state);
    }
}
