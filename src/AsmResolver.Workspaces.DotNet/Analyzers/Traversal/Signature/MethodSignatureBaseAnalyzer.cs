using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Signature
{
    /// <summary>
    /// Provides a default implementation for an <see cref="MethodSignatureBase"/> analyzer.
    /// </summary>
    public class MethodSignatureBaseAnalyzer : ObjectAnalyzer<MethodSignatureBase>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, MethodSignatureBase subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.ScheduleForAnalysis(subject.ReturnType);
                for (int i = 0; i < subject.ParameterTypes.Count; i++)
                    context.ScheduleForAnalysis(subject.ParameterTypes[i]);
                for (int i = 0; i < subject.SentinelParameterTypes.Count; i++)
                    context.ScheduleForAnalysis(subject.SentinelParameterTypes[i]);
            }
        }
    }
}
