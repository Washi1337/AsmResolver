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
    public class GenericTypeActivator : ITypeSignatureVisitor<TypeSignature>
    {
        private readonly GenericContext _context;

        /// <summary>
        /// Creates a new instance of the <see cref="GenericTypeActivator"/> class.
        /// </summary>
        /// <param name="context">The generic context to put the type signature in.</param>
        public GenericTypeActivator(in GenericContext context)
        {
            _context = context;
        }

        public FieldSignature InstantiateFieldSignature(FieldSignature fieldSignature)
        {
            return new FieldSignature(
                fieldSignature.Attributes, 
                fieldSignature.FieldType.AcceptVisitor(this));
        }

        public MethodSignature InstantiateMethodSignature(MethodSignature signature)
        {
            var result = new MethodSignature(
                signature.Attributes,
                signature.ReturnType.AcceptVisitor(this),
                signature.ParameterTypes.Select(t => t.AcceptVisitor(this)));
            result.GenericParameterCount = signature.GenericParameterCount;

            if (signature.IncludeSentinel)
            {
                result.IncludeSentinel = signature.IncludeSentinel;
                foreach (var sentinelType in signature.SentinelParameterTypes)
                    result.SentinelParameterTypes.Add(sentinelType.AcceptVisitor(this));
            }

            return result;
        }

        /// <inheritdoc />
        public TypeSignature VisitArrayType(ArrayTypeSignature signature)
        {
            var result = new ArrayTypeSignature(signature.BaseType.AcceptVisitor(this));
            for (int i = 0; i < signature.Dimensions.Count; i++)
                result.Dimensions.Add(signature.Dimensions[i]);
            return result;
        }

        /// <inheritdoc />
        public TypeSignature VisitBoxedType(BoxedTypeSignature signature)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new BoxedTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitByReferenceType(ByReferenceTypeSignature signature)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new ByReferenceTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitCorLibType(CorLibTypeSignature signature)
        {
            return signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitCustomModifierType(CustomModifierTypeSignature signature)
        {
            return new CustomModifierTypeSignature(
                signature.ModifierType,
                signature.IsRequired,
                signature.BaseType.AcceptVisitor(this));
        }

        /// <inheritdoc />
        public TypeSignature VisitGenericInstanceType(GenericInstanceTypeSignature signature)
        {
            var result = new GenericInstanceTypeSignature(signature.GenericType, signature.IsValueType);
            for (int i = 0; i < signature.TypeArguments.Count; i++)
                result.TypeArguments.Add(signature.TypeArguments[i].AcceptVisitor(this));
            return result;
        }

        /// <inheritdoc />
        public TypeSignature VisitGenericParameter(GenericParameterSignature signature)
        {
            return _context.GetTypeArgument(signature);
        }

        /// <inheritdoc />
        public TypeSignature VisitPinnedType(PinnedTypeSignature signature)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new PinnedTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitPointerType(PointerTypeSignature signature)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new PointerTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitSentinelType(SentinelTypeSignature signature)
        {
            return signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitSzArrayType(SzArrayTypeSignature signature)
        {
            var instantiatedBaseType = signature.BaseType.AcceptVisitor(this);
            return !ReferenceEquals(instantiatedBaseType, signature.BaseType)
                ? new SzArrayTypeSignature(instantiatedBaseType)
                : signature;
        }

        /// <inheritdoc />
        public TypeSignature VisitTypeDefOrRef(TypeDefOrRefSignature signature)
        {
            return signature;
        }
    }
}