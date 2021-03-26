using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="CilLocalVariable"/> analyzer.
    /// </summary>
    public class CilLocalVariableAnalyzer : ObjectAnalyzer<CilLocalVariable>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, CilLocalVariable subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.VariableType);
            }
        }
    }
}
