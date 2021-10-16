using AsmResolver.DotNet.Code.Cil;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="CilExceptionHandler"/> analyzer.
    /// </summary>
    public class ExceptionHandlerAnalyzer : ObjectAnalyzer<CilExceptionHandler>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, CilExceptionHandler subject)
        {
            if (subject.ExceptionType is not null && context.HasAnalyzers(subject.ExceptionType.GetType()))
            {
                context.ScheduleForAnalysis(subject.ExceptionType);
            }
        }
    }
}
