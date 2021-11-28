using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="MethodSpecification"/> analyzer.
    /// </summary>
    public class MethodSpecificationAnalyzer : ObjectAnalyzer<MethodSpecification>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, MethodSpecification subject)
        {
            if (subject.Method is not null && context.HasAnalyzers(subject.Method.GetType()))
            {
                context.ScheduleForAnalysis(subject.Method);
            }
            if (subject.Signature is not null && context.HasAnalyzers(typeof(GenericInstanceMethodSignature)))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }

            if (subject.DeclaringType is not null && context.HasAnalyzers(subject.DeclaringType.GetType()))
            {
                context.ScheduleForAnalysis(subject.DeclaringType);
            }

            if (subject.Resolve() is { } definition)
            {
                var specification = context.Workspace.Index.GetOrCreateNode(subject);
                var definitionNode = context.Workspace.Index.GetOrCreateNode(definition);
                definitionNode.ForwardRelations.Add(DotNetRelations.ReferenceSpecification, specification);
            }
        }
    }
}
