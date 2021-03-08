using System.Linq;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Provides a mechanism for substituting generic type parameters in a type signature with arguments.
    /// </summary>
    /// <remarks>
    /// When the type signature does not contain any generic parameter, this activator might return the current
    /// instance of the type signature, to preserve heap allocations.
    /// </remarks>
    public class GenericTypeActivator : ITypeSignatureVisitor<GenericContext, TypeSignature>
    {
        /// <summary>
        /// Instantiates a new field signature, substituting any generic type parameter in the signature with
        /// the activation context.
        /// </summary>
        /// <param name="signature">The signature to activate.</param>
        /// <param name="context">The generic context to put the type signature in.</param>
        /// <returns>The activated signature.</returns>
        public FieldSignature InstantiateFieldSignature(FieldSignature signature, GenericContext context)
        {
            return new FieldSignature(
                signature.Attributes,
                signature.FieldType.AcceptVisitor(this, context));
        }

        /// <summary>
        /// Instantiates a new method signature, substituting any generic type parameter in the signature with
        /// the activation context.
        /// </summary>
        /// <param name="signature">The signature to activate.</param>
        /// <param name="context">The generic context to put the type signature in.</param>
        /// <returns>The activated signature.</returns>
        public PropertySignature InstantiatePropertySignature(PropertySignature signature, GenericContext context)
        {
            var result = new PropertySignature(signature.Attributes);
            InstantiateMethodSignatureBase(signature, result, context);
            return result;
        }

        /// <summary>
        /// Instantiates a new method signature, substituting any generic type parameter in the signature with
        /// the activation context.
        /// </summary>
        /// <param name="signature">The signature to activate.</param>
        /// <param name="context">The generic context to put the type signature in.</param>
        /// <returns>The activated signature.</returns>
        public MethodSignature InstantiateMethodSignature(MethodSignature signature, GenericContext context)
        {
            var result = new MethodSignature(signature.Attributes);
            InstantiateMethodSignatureBase(signature, result, context);
            result.GenericParameterCount = signature.GenericParameterCount;

            if (signature.IncludeSentinel)
            {
                result.IncludeSentinel = signature.IncludeSentinel;
                foreach (var sentinelType in signature.SentinelParameterTypes)
                    result.SentinelParameterTypes.Add(sentinelType.AcceptVisitor(this, context));
            }

            return result;
        }

        private void InstantiateMethodSignatureBase(MethodSignatureBase signature, MethodSignatureBase result, GenericContext context)
        {
            result.ReturnType = signature.ReturnType.AcceptVisitor(this, context);
            foreach (var parameterType in signature.ParameterTypes)
                result.ParameterTypes.Add(parameterType.AcceptVisitor(this, context));
        }

        /// <inheritdoc />
        public TypeSignature VisitArrayType(ArrayTypeSignature signature, GenericContext context)
        {
            var result = new ArrayTypeSignature(signature.BaseType.AcceptVisitor(this, context));
            for (int i = 0; i < signature.Dimensions.Count; i++)
                result.Dimensions.Add(signature.Dimensions[i]);
            return result;
        }

        /// <inheritdoc />
        public TypeSignature VisitBoxedType(BoxedTypeSignature signature, GenericContext context)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this, context);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new BoxedTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitByReferenceType(ByReferenceTypeSignature signature, GenericContext context)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this, context);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new ByReferenceTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitCorLibType(CorLibTypeSignature signature, GenericContext context)
        {
            return signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitCustomModifierType(CustomModifierTypeSignature signature, GenericContext context)
        {
            return new CustomModifierTypeSignature(
                signature.ModifierType,
                signature.IsRequired,
                signature.BaseType.AcceptVisitor(this, context));
        }

        /// <inheritdoc />
        public TypeSignature VisitGenericInstanceType(GenericInstanceTypeSignature signature, GenericContext context)
        {
            var result = new GenericInstanceTypeSignature(signature.GenericType, signature.IsValueType);
            for (int i = 0; i < signature.TypeArguments.Count; i++)
                result.TypeArguments.Add(signature.TypeArguments[i].AcceptVisitor(this, context));
            return result;
        }

        /// <inheritdoc />
        public TypeSignature VisitGenericParameter(GenericParameterSignature signature, GenericContext context)
        {
            return context.GetTypeArgument(signature);
        }

        /// <inheritdoc />
        public TypeSignature VisitPinnedType(PinnedTypeSignature signature, GenericContext context)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this, context);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new PinnedTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitPointerType(PointerTypeSignature signature, GenericContext context)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this, context);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new PointerTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitSentinelType(SentinelTypeSignature signature, GenericContext context)
        {
            return signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitSzArrayType(SzArrayTypeSignature signature, GenericContext context)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this, context);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new SzArrayTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitTypeDefOrRef(TypeDefOrRefSignature signature, GenericContext context)
        {
            return signature;
        }
    }
}
