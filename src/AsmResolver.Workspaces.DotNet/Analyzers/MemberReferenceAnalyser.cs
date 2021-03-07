using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    /// <summary>
    /// Analyzes a <see cref="MemberReference"/> for its definitions
    /// </summary>
    public class MemberReferenceAnalyser : ObjectAnalyzer<MemberReference>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, MemberReference subject)
        {
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