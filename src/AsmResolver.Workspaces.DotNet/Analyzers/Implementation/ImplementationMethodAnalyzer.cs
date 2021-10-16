using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Implementation
{
    /// <summary>
    /// Analyzes a <see cref="MethodDefinition"/> for implicit base definitions, such as abstract methods in
    /// base classes or methods in implemented interfaces.
    /// </summary>
    internal class ImplementationMethodAnalyzer : ObjectAnalyzer<MethodDefinition>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, MethodDefinition subject)
        {
            if (!subject.IsVirtual)
                return;

            var index = context.Workspace.Index;
            var node = index.GetOrCreateNode(subject);

            foreach (var baseMethod in subject.FindBaseMethods(context.Workspace.Index))
            {
                var candidateNode = index.GetOrCreateNode(baseMethod);
                node.ForwardRelations.Add(DotNetRelations.ImplementationMethod, candidateNode);
            }
        }
    }
}
