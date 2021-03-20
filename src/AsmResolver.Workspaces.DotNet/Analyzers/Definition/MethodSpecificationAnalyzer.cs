using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="MethodSpecification"/> analyzer.
    /// </summary>
    public class MethodSpecificationAnalyzer : ObjectAnalyzer<MethodSpecification>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, MethodSpecification subject)
        {
            if (context.HasAnalyzers(subject.Method.GetType()))
            {
                context.SchedulaForAnalysis(subject.Method);
            }
            if (context.HasAnalyzers(typeof(GenericInstanceMethodSignature)))
            {
                context.SchedulaForAnalysis(subject.Signature);
            }
        }
    }
}
