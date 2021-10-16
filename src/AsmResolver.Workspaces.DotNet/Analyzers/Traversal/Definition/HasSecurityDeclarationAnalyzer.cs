using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Analyzes a <see cref="IHasSecurityDeclaration"/> for its definitions
    /// </summary>
    public class HasSecurityDeclarationAnalyzer : ObjectAnalyzer<IHasSecurityDeclaration>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, IHasSecurityDeclaration subject)
        {
            if (context.HasAnalyzers(typeof(SecurityDeclaration)))
            {
                for (int i = 0; i < subject.SecurityDeclarations.Count; i++)
                    context.ScheduleForAnalysis(subject.SecurityDeclarations[i]);
            }
        }
    }
}
