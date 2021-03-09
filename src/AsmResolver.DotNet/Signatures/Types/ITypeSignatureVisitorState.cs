namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Provides members for visiting type signatures.
    /// </summary>
    /// <typeparam name="TSTate">The type of additional state.</typeparam>
    /// <typeparam name="TResult">The type of value to return.</typeparam>
    public interface ITypeSignatureVisitor<in TSTate, out TResult>
    {
        /// <summary>
        /// Visits an instance of an <see cref="ArrayTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitArrayType(ArrayTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="BoxedTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitBoxedType(BoxedTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="ByReferenceTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitByReferenceType(ByReferenceTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="CorLibTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitCorLibType(CorLibTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="CustomModifierTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitCustomModifierType(CustomModifierTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="GenericInstanceTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitGenericInstanceType(GenericInstanceTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="GenericParameterSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitGenericParameter(GenericParameterSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="PinnedTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitPinnedType(PinnedTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="PointerTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitPointerType(PointerTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="SentinelTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitSentinelType(SentinelTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="SzArrayTypeSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitSzArrayType(SzArrayTypeSignature signature, TSTate state);

        /// <summary>
        /// Visits an instance of a <see cref="TypeDefOrRefSignature"/>.
        /// </summary>
        /// <param name="signature">The signature to visit.</param>
        /// <param name="state">Additional state.</param>
        /// <returns>The result provided by the visitor.</returns>
        TResult VisitTypeDefOrRef(TypeDefOrRefSignature signature, TSTate state);
    }

}
