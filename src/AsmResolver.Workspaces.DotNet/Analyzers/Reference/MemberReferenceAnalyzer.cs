using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Reference
{
    /// <summary>
    /// Analyzes a <see cref="MemberReference"/> for its definitions
    /// </summary>
    public class MemberReferenceAnalyzer : ObjectAnalyzer<MemberReference>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, MemberReference subject)
        {

            if (subject.DeclaringType is not null && context.HasAnalyzers(subject.DeclaringType.GetType()))
            {
                context.ScheduleForAnalysis(subject.DeclaringType);
            }

            if (subject.Signature is not null && context.HasAnalyzers(subject.Signature.GetType()))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }

            if (context.Workspace is not DotNetWorkspace workspace)
                return;

            var definition = subject.Resolve();
            if (definition is not { Module: { Assembly: { } } } || !workspace.Assemblies.Contains(definition.Module.Assembly))
                return; //TODO: Maybe add some warning log?

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(definition);
            var candidateNode = index.GetOrCreateNode(subject);
            node.ForwardRelations.Add(DotNetRelations.ReferenceMember, candidateNode);
        }
    }
}
