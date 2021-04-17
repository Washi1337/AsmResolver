using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Signature
{
    /// <summary>
    /// Provides a default implementation for an <see cref="TypeSignature"/> analyzer.
    /// </summary>
    public class TypeSignatureAnalyzer : ObjectAnalyzer<TypeSignature>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, TypeSignature subject)
        {
            subject.AcceptVisitor(TypeSpecificationMemberScheduler.Instance, context);
        }

        sealed class TypeSpecificationMemberScheduler : ITypeSignatureVisitor<AnalysisContext, TypeSignature>
        {
            public static TypeSpecificationMemberScheduler Instance
            {
                get;
            } = new();

            /// <inheritdoc />
            public TypeSignature VisitArrayType(ArrayTypeSignature signature, AnalysisContext context)
            {
                signature.BaseType.AcceptVisitor(this, context);
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitBoxedType(BoxedTypeSignature signature, AnalysisContext context)
            {
                signature.BaseType.AcceptVisitor(this, context);
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitByReferenceType(ByReferenceTypeSignature signature, AnalysisContext context)
            {
                signature.BaseType.AcceptVisitor(this, context);
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitCorLibType(CorLibTypeSignature signature, AnalysisContext context)
            {
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitCustomModifierType(CustomModifierTypeSignature signature, AnalysisContext context)
            {
                if (signature.ModifierType is not null && context.HasAnalyzers(signature.ModifierType.GetType()))
                {
                    context.ScheduleForAnalysis(signature.ModifierType);
                }
                signature.BaseType.AcceptVisitor(this, context);
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitGenericInstanceType(GenericInstanceTypeSignature signature,
                AnalysisContext context)
            {
                if (signature.GenericType is not null && context.HasAnalyzers(signature.GenericType.GetType()))
                {
                    context.ScheduleForAnalysis(signature.GenericType);
                }

                for (int i = 0; i < signature.TypeArguments.Count; i++)
                    signature.TypeArguments[i].AcceptVisitor(this, context);

                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitGenericParameter(GenericParameterSignature signature, AnalysisContext context)
            {
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitPinnedType(PinnedTypeSignature signature, AnalysisContext context)
            {
                signature.BaseType.AcceptVisitor(this, context);
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitPointerType(PointerTypeSignature signature, AnalysisContext context)
            {
                signature.BaseType.AcceptVisitor(this, context);
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitSentinelType(SentinelTypeSignature signature, AnalysisContext context)
            {
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitSzArrayType(SzArrayTypeSignature signature, AnalysisContext context)
            {
                signature.BaseType.AcceptVisitor(this, context);
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitTypeDefOrRef(TypeDefOrRefSignature signature, AnalysisContext context)
            {
                if (signature.Type is not null && context.HasAnalyzers(signature.Type.GetType()))
                {
                    context.ScheduleForAnalysis(signature.Type);
                }
                return signature;
            }
        }
    }
}
