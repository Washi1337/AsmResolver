using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="FieldDefinition"/> analyzer.
    /// </summary>
    public class FieldAnalyzer : ObjectAnalyzer<FieldDefinition>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, FieldDefinition subject)
        {
            // Schedule signature for analysis.
            if (context.HasAnalyzers(typeof(FieldSignature)))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }
        }
    }
}
