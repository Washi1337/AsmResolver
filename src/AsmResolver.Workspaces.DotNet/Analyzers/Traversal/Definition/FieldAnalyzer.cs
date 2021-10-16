using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="FieldDefinition"/> analyzer.
    /// </summary>
    public class FieldAnalyzer : ObjectAnalyzer<FieldDefinition>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, FieldDefinition subject)
        {
            // Schedule signature for analysis.
            if (subject.Signature is not null && context.HasAnalyzers(typeof(FieldSignature)))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }
        }
    }
}
