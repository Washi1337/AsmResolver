using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Signature
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
