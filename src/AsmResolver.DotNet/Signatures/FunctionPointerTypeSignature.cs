using System;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents the type of an object referencing a function or method pointer.
    /// </summary>
    public class FunctionPointerTypeSignature : TypeSignature
    {
        /// <summary>
        /// Creates a new function pointer type signature.
        /// </summary>
        /// <param name="signature">The signature of the function pointer.</param>
        public FunctionPointerTypeSignature(MethodSignature signature)
        {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.FnPtr;

        /// <summary>
        /// Gets or sets the signature of the function or method that is referenced by the object.
        /// </summary>
        public MethodSignature Signature
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Name => $"method {Signature}";

        /// <inheritdoc />
        public override string? Namespace => null;

        /// <inheritdoc />
        public override IResolutionScope? Scope => Signature.ReturnType.Scope;

        /// <inheritdoc />
        public override bool IsValueType => true;

        /// <inheritdoc />
        public override ITypeDefOrRef? GetUnderlyingTypeDefOrRef() =>
            Signature.ReturnType.ContextModule?.CorLibTypeFactory.IntPtr.Type;

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module) => Signature.IsImportedInModule(module);

        /// <inheritdoc />
        protected override bool IsDirectlyCompatibleWith(TypeSignature other, SignatureComparer comparer)
        {
            if (base.IsDirectlyCompatibleWith(other, comparer))
                return true;

            if (other is not FunctionPointerTypeSignature {Signature: { } otherSignature}
                || Signature.GenericParameterCount != otherSignature.GenericParameterCount
                || Signature.ParameterTypes.Count != otherSignature.ParameterTypes.Count
                || !Signature.ReturnType.IsAssignableTo(otherSignature.ReturnType, comparer))
            {
                return false;
            }

            for (int i = 0; i < Signature.ParameterTypes.Count; i++)
            {
                if (!Signature.ParameterTypes[i].IsAssignableTo(otherSignature.ParameterTypes[i], comparer))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
        {
            context.Writer.WriteByte((byte) ElementType);
            Signature.Write(context);
        }

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) =>
            visitor.VisitFunctionPointerType(this);

        /// <inheritdoc />
        public override TResult AcceptVisitor<TState, TResult>(ITypeSignatureVisitor<TState, TResult> visitor, TState state) =>
            visitor.VisitFunctionPointerType(this, state);
    }
}
