using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Signature
{
    /// <summary>
    /// Provides a default implementation for an <see cref="MethodSignatureBase"/> analyzer.
    /// </summary>
    public class MethodSignatureBaseAnalyzer : ObjectAnalyzer<MethodSignatureBase>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, MethodSignatureBase subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.ReturnType);
                for (int i = 0; i < subject.ParameterTypes.Count; i++)
                    context.SchedulaForAnalysis(subject.ParameterTypes[i]);
                for (int i = 0; i < subject.SentinelParameterTypes.Count; i++)
                    context.SchedulaForAnalysis(subject.SentinelParameterTypes[i]);
            }
        }
    }
}
