using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.Workspaces.DotNet.Analyzers.Implementation;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Reference
{
    /// <summary>
    /// Provides a default implementation for an <see cref="TypeSpecification"/> analyzer.
    /// </summary>
    public class TypeSpecificationAnalyzer : ObjectAnalyzer<TypeSpecification>
    {
        private readonly SignatureComparer _comparer = new();

        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, TypeSpecification subject)
        {

            if (subject.Signature is not null && context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }

            if (subject.DeclaringType is not null && context.HasAnalyzers(subject.DeclaringType.GetType()))
            {
                context.ScheduleForAnalysis(subject.DeclaringType);
            }

            if(!context.Workspace.ContainsSubjectAssembly(subject))
                return;
            if (subject.Resolve() is not {} definition)
                return;

            var specification = context.Workspace.Index.GetOrCreateNode(subject);
            var typeNode = context.Workspace.Index.GetOrCreateNode(definition);
            typeNode.ForwardRelations.Add(DotNetRelations.ReferenceType, specification);
        }
    }
}
