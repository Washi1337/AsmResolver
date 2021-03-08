using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    /// <summary>
    /// Analyzes a <see cref="TypeReference"/> for its definitions
    /// </summary>
    public class TypeReferenceAnalyser : ObjectAnalyzer<TypeReference>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, TypeReference subject)
        {
            if(context.Workspace is not DotNetWorkspace workspace)
                return;
            
            var definition = subject.Resolve();
            if(definition is null || !workspace.Assemblies.Contains(definition.Module.Assembly))
                return;
            
            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);
            var candidateNode = index.GetOrCreateNode(definition);
            node.AddRelation(DotNetRelations.ReferenceType, candidateNode);
        }
    }
}