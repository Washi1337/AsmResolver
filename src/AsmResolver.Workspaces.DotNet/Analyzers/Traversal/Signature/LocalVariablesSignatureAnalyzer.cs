using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Signature
{
    /// <summary>
    /// Provides a default implementation for an <see cref="LocalVariablesSignature"/> analyzer.
    /// </summary>
    public class LocalVariablesSignatureAnalyzer : ObjectAnalyzer<LocalVariablesSignature>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, LocalVariablesSignature subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                for (int i = 0; i < subject.VariableTypes.Count; i++)
                {
                    var variableType = subject.VariableTypes[i];
                    context.ScheduleForAnalysis(variableType);
                }
            }
        }
    }
}
