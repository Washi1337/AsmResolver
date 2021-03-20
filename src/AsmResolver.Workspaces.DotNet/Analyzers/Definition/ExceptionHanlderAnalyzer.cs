using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="CilExceptionHandler"/> analyzer.
    /// </summary>
    public class ExceptionHandlerAnalyzer : ObjectAnalyzer<CilExceptionHandler>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, CilExceptionHandler subject)
        {
            if (subject.ExceptionType is not null && context.HasAnalyzers(subject.ExceptionType.GetType()))
            {
                context.SchedulaForAnalysis(subject.ExceptionType);
            }
        }
    }
}
