using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Reference
{
    /// <summary>
    /// Analyzes a <see cref="AssemblyReference"/> for its definitions
    /// </summary>
    public class AssemblyReferenceAnalyzer : ObjectAnalyzer<AssemblyReference>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, AssemblyReference subject)
        {
            if (context.Workspace is not DotNetWorkspace workspace)
                return;

            if(!context.Workspace.ContainsSubjectAssembly(subject))
                return;
            if (subject.Resolve() is not {} definition)
                return;

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(definition);
            var candidateNode = index.GetOrCreateNode(subject);
            node.ForwardRelations.Add(DotNetRelations.ReferenceAssembly, candidateNode);
        }
    }
}
