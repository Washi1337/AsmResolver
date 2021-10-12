using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="MethodImplementation"/> analyzer.
    /// </summary>
    public class MethodImplementationAnalyzer : ObjectAnalyzer<MethodImplementation>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, MethodImplementation subject)
        {
            if (subject.Declaration is not null && context.HasAnalyzers(subject.Declaration.GetType()))
            {
                context.ScheduleForAnalysis(subject.Declaration);
            }
            if (subject.Body is not null && context.HasAnalyzers(subject.Body.GetType()))
            {
                context.ScheduleForAnalysis(subject.Body);
            }
        }
    }
}
