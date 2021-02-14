using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.Dotnet.Analyzers
{
    /// <summary>
    /// Provides a default implementation for an <see cref="AssemblyDefinition"/> analyzer.
    /// </summary>
    public class AssemblyAnalyzer : ObjectAnalyzer<AssemblyDefinition>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, AssemblyDefinition subject)
        {
            context.Workspace.Index.GetOrCreateNode(subject);

            for (int i = 0; i < subject.Modules.Count; i++)
                context.SchedulaForAnalysis(subject.Modules[i]);
        }
    }
}
