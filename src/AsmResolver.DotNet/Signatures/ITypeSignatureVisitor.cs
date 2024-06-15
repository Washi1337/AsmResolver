namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides members for visiting type signatures.
    /// </summary>
    /// <typeparam name="TResult">The type of value to return.</typeparam>
    public interface ITypeSignatureVisitor<out TResult>
    {
        /// <summary>
        /// Visits an instance of an <see cref="ArrayTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitArrayType(ArrayTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="BoxedTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitBoxedType(BoxedTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="ByReferenceTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitByReferenceType(ByReferenceTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="CorLibTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitCorLibType(CorLibTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="CustomModifierTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitCustomModifierType(CustomModifierTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="GenericInstanceTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitGenericInstanceType(GenericInstanceTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="GenericParameterSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitGenericParameter(GenericParameterSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="PinnedTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitPinnedType(PinnedTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="PointerTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitPointerType(PointerTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="SentinelTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitSentinelType(SentinelTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="SzArrayTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitSzArrayType(SzArrayTypeSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="TypeDefOrRefSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitTypeDefOrRef(TypeDefOrRefSignature signature);

        /// <summary>
        /// Visits an instance of a <see cref="FunctionPointerTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitFunctionPointerType(FunctionPointerTypeSignature signature);
    }

}
