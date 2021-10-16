using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Signature
{
    /// <summary>
    /// Provides a default implementation for an <see cref="StandAloneSignature"/> analyzer.
    /// </summary>
    public class StandaloneSignatureAnalyzer : ObjectAnalyzer<StandAloneSignature>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, StandAloneSignature subject)
        {
            if (subject.Signature is not null && context.HasAnalyzers(subject.Signature.GetType()))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }
        }
    }
}
