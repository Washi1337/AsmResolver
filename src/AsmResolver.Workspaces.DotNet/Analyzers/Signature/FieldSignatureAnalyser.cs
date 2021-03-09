using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Signature
{
    /// <summary>
    /// Provides a default implementation for an <see cref="FieldSignature"/> analyzer.
    /// </summary>
    public class FieldSignatureAnalyser : ObjectAnalyzer<FieldSignature>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, FieldSignature subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.FieldType);
            }
        }
    }
}
