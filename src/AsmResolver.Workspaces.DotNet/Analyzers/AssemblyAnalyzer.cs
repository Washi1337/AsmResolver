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

            // Schedule all the defined modules in the module for analysis.
            if (context.HasAnalyzers(typeof(AssemblyDefinition)))
            {
                for (int i = 0; i < subject.Modules.Count; i++)
                    context.SchedulaForAnalysis(subject.Modules[i]);
            }
        }
    }
}
