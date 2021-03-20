using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Reference
{
    /// <summary>
    /// Analyzes a <see cref="AssemblyReference"/> for its definitions
    /// </summary>
    public class AssemblyReferenceAnalyzer : ObjectAnalyzer<AssemblyReference>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, AssemblyReference subject)
        {
            if(context.Workspace is not DotNetWorkspace workspace)
                return;

            var definition = subject.Resolve();
            if(definition is null || !workspace.Assemblies.Contains(definition))
                return; //TODO: Maybe add some warning log?

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);
            var candidateNode = index.GetOrCreateNode(definition);
            node.AddRelation(DotNetRelations.ReferenceAssembly, candidateNode);
        }
    }
}