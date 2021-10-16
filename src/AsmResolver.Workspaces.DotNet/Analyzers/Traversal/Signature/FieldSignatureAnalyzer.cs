using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Signature
{
    /// <summary>
    /// Provides a default implementation for an <see cref="FieldSignature"/> analyzer.
    /// </summary>
    public class FieldSignatureAnalyzer : ObjectAnalyzer<FieldSignature>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, FieldSignature subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.ScheduleForAnalysis(subject.FieldType);
            }
        }
    }
}
