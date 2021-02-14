using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.Dotnet.Analyzers
{
    /// <summary>
    /// Provides a default implementation for an <see cref="ModuleDefinition"/> analyzer.
    /// </summary>
    public class ModuleAnalyzer : ObjectAnalyzer<ModuleDefinition>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, ModuleDefinition subject)
        {
            context.Workspace.Index.GetOrCreateNode(subject);

            foreach (var type in subject.GetAllTypes())
                context.SchedulaForAnalysis(type);
        }
    }
}
