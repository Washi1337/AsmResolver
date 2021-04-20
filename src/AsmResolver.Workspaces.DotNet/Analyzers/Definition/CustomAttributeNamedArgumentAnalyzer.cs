using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="CustomAttributeNamedArgument"/> for its definitions
    /// </summary>
    public class CustomAttributeNamedArgumentAnalyzer : ObjectAnalyzer<CustomAttributeNamedArgument>
    {
        protected override void Analyze(AnalysisContext context, CustomAttributeNamedArgument subject)
        {
            if (context.HasAnalyzers(typeof(CustomAttributeArgument)))
            {
                context.ScheduleForAnalysis(subject.Argument);
            }
        }
    }
}
