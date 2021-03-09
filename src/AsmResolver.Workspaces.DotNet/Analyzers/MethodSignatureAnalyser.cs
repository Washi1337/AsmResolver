using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    /// <summary>
    /// Provides a default implementation for an <see cref="MethodSignature"/> analyzer.
    /// </summary>
    public class MethodSignatureAnalyser : ObjectAnalyzer<MethodSignature>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, MethodSignature subject)
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
