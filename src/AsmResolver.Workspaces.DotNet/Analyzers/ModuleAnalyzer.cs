using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers
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

            // Schedule all the defined types in the module for analysis.
            if (context.HasAnalyzers(typeof(TypeDefinition)))
            {
                foreach (var type in subject.GetAllTypes())
                    context.SchedulaForAnalysis(type);
            }
        }
    }
}
