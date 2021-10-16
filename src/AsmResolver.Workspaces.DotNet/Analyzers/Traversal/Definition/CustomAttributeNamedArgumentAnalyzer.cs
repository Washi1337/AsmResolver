using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Analyzes a <see cref="CustomAttributeNamedArgument"/> for its definitions
    /// </summary>
    public class CustomAttributeNamedArgumentAnalyzer : ObjectAnalyzer<CustomAttributeNamedArgument>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, CustomAttributeNamedArgument subject)
        {
            if (context.HasAnalyzers(typeof(CustomAttributeArgument)))
            {
                context.ScheduleForAnalysis(subject.Argument);
            }
        }
    }
}
