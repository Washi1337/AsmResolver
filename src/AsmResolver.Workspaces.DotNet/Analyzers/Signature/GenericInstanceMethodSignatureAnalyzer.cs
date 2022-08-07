using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Signature
{
    /// <summary>
    /// Provides a default implementation for an <see cref="GenericInstanceMethodSignature"/> analyzer.
    /// </summary>
    public class GenericInstanceMethodSignatureAnalyzer : ObjectAnalyzer<GenericInstanceMethodSignature>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, GenericInstanceMethodSignature subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                for (var i = 0; i < subject.TypeArguments.Count; i++)
                {
                    context.ScheduleForAnalysis(subject.TypeArguments[i]);
                }
            }
        }
    }
}
