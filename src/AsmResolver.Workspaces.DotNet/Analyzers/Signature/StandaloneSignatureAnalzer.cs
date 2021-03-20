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
        public override void Analyze(AnalysisContext context, StandAloneSignature subject)
        {
            if (context.HasAnalyzers(subject.Signature.GetType()))
            {
                context.SchedulaForAnalysis(subject.Signature);
            }
        }
    }
}
