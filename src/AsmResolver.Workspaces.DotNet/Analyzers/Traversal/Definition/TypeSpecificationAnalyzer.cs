using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="TypeSpecification"/> analyzer.
    /// </summary>
    public class TypeSpecificationAnalyzer : ObjectAnalyzer<TypeSpecification>
    {
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

            if (subject.Resolve() is { } definition)
            {
                var specification = context.Workspace.Index.GetOrCreateNode(subject);
                var definitionNode = context.Workspace.Index.GetOrCreateNode(definition);
                definitionNode.ForwardRelations.Add(DotNetRelations.ReferenceType, specification);
            }

        }
    }
}
