using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="CilLocalVariable"/> analyzer.
    /// </summary>
    public class CilLocalVariableAnalyzer : ObjectAnalyzer<CilLocalVariable>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, CilLocalVariable subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.ScheduleForAnalysis(subject.VariableType);
            }
        }
    }
}
