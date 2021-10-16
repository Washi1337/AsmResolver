using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="AssemblyDefinition"/> analyzer.
    /// </summary>
    public class AssemblyAnalyzer : ObjectAnalyzer<AssemblyDefinition>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, AssemblyDefinition subject)
        {
            context.Workspace.Index.GetOrCreateNode(subject);

            // Schedule all the defined modules in the assembly for analysis.
            if (context.HasAnalyzers(typeof(AssemblyDefinition)))
            {
                for (int i = 0; i < subject.Modules.Count; i++)
                    context.ScheduleForAnalysis(subject.Modules[i]);
            }
        }
    }
}
