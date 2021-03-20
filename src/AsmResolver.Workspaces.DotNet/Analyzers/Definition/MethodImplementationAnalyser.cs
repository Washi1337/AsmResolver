using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="MethodImplementation"/> analyzer.
    /// </summary>
    public class MethodImplementationAnalyser : ObjectAnalyzer<MethodImplementation>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, MethodImplementation subject)
        {
            if (context.HasAnalyzers(subject.Declaration.GetType()))
            {
                context.SchedulaForAnalysis(subject.Declaration);
            }
            if (context.HasAnalyzers(subject.Body.GetType()))
            {
                context.SchedulaForAnalysis(subject.Body);
            }
        }
    }
}
