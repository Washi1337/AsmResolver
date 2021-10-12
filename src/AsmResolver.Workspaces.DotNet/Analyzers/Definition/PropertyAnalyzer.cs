using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="PropertyDefinition"/> analyzer.
    /// </summary>
    public class PropertyAnalyzer : ObjectAnalyzer<PropertyDefinition>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, PropertyDefinition subject)
        {
            // Schedule signature for analysis.
            if (subject.Signature is not null && context.HasAnalyzers(typeof(PropertySignature)))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }
        }
    }
}
