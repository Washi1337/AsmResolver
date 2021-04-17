using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
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
                    context.ScheduleForAnalysis(type);
            }

            if (context.HasAnalyzers(typeof(FileReference)))
            {
                for (int i = 0; i < subject.FileReferences.Count; i++)
                    context.ScheduleForAnalysis(subject.FileReferences[i]);
            }

            if (context.HasAnalyzers(typeof(AssemblyReference)))
            {
                for (int i = 0; i < subject.AssemblyReferences.Count; i++)
                    context.ScheduleForAnalysis(subject.AssemblyReferences[i]);
            }

            if (context.HasAnalyzers(typeof(ModuleReference)))
            {
                for (int i = 0; i < subject.ModuleReferences.Count; i++)
                    context.ScheduleForAnalysis(subject.ModuleReferences[i]);
            }

            if (context.HasAnalyzers(typeof(ManifestResource)))
            {
                for (int i = 0; i < subject.Resources.Count; i++)
                    context.ScheduleForAnalysis(subject.Resources[i]);
            }

            if (context.HasAnalyzers(typeof(ExportedType)))
            {
                for (int i = 0; i < subject.ExportedTypes.Count; i++)
                    context.ScheduleForAnalysis(subject.ExportedTypes[i]);
            }
        }
    }
}
