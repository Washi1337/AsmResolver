using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Analyzes a <see cref="IHasCustomAttribute"/> for its definitions
    /// </summary>
    public class HasCustomAttributeAnalyzer : ObjectAnalyzer<IHasCustomAttribute>
    {

        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, IHasCustomAttribute subject)
        {
            if (context.HasAnalyzers(typeof(CustomAttribute)))
            {
                for (int i = 0; i < subject.CustomAttributes.Count; i++)
                    context.ScheduleForAnalysis(subject.CustomAttributes[i]);
            }
        }
    }
}
