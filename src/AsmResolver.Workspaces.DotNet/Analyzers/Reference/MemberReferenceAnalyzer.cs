using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Reference
{
    /// <summary>
    /// Analyzes a <see cref="MemberReference"/> for its definitions
    /// </summary>
    public class MemberReferenceAnalyzer : ObjectAnalyzer<MemberReference>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, MemberReference subject)
        {

            if (subject.DeclaringType is not null && context.HasAnalyzers(subject.DeclaringType.GetType()))
            {
                context.SchedulaForAnalysis(subject.DeclaringType);
            }

            if (context.HasAnalyzers(subject.Signature.GetType()))
            {
                context.SchedulaForAnalysis(subject.Signature);
            }

            if(context.Workspace is not DotNetWorkspace workspace)
                return;

            var definition = subject.Resolve();
            if(definition is null || !workspace.Assemblies.Contains(definition.Module.Assembly))
                return; //TODO: Maybe add some warning log?

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);
            var candidateNode = index.GetOrCreateNode(definition);
            node.AddRelation(DotNetRelations.ReferenceMember, candidateNode);
        }
    }
}
