using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    /// <summary>
    /// Provides a default implementation for an <see cref="TypeSignature"/> analyzer.
    /// </summary>
    public class TypeSpecificationAnalyser : ObjectAnalyzer<TypeSignature>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, TypeSignature subject)
        {
            subject.AcceptVisitor(TypeSpecificationMemberScheduler.Instance, context);
        }

        private class TypeSpecificationMemberScheduler : ITypeSignatureVisitor<AnalysisContext, TypeSignature>
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
                if (context.HasAnalyzers(typeof(ITypeDefOrRef)))
                {
                    context.SchedulaForAnalysis(signature.Type);
                }
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitCustomModifierType(CustomModifierTypeSignature signature, AnalysisContext context)
            {
                if (context.HasAnalyzers(typeof(ITypeDefOrRef)))
                {
                    context.SchedulaForAnalysis(signature.ModifierType);
                }
                signature.BaseType.AcceptVisitor(this, context);
                return signature;
            }

            /// <inheritdoc />
            public TypeSignature VisitGenericInstanceType(GenericInstanceTypeSignature signature,
                AnalysisContext context)
            {
                if (context.HasAnalyzers(typeof(ITypeDefOrRef)))
                {
                    context.SchedulaForAnalysis(signature.GenericType);
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
                if (context.HasAnalyzers(typeof(ITypeDefOrRef)))
                {
                    context.SchedulaForAnalysis(signature.Type);
                }
                return signature;
            }
        }
    }
}
