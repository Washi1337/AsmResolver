using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="MethodDefinition"/> analyzer.
    /// </summary>
    public class MethodAnalyzer : ObjectAnalyzer<MethodDefinition>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, MethodDefinition subject)
        {
            // Schedule parameters for analysis.
            if (context.HasAnalyzers(typeof(ParameterDefinition)))
            {
                for (int i = 0; i < subject.ParameterDefinitions.Count; i++)
                    context.ScheduleForAnalysis(subject.ParameterDefinitions[i]);
            }

            // Schedule signature for analysis.
            if (subject.Signature is not null && context.HasAnalyzers(typeof(MethodSignature)))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }

            // Schedule method body for analysis.
            if (subject.IsIL && subject.CilMethodBody is not null && context.HasAnalyzers(typeof(CilMethodBody)))
            {
                context.ScheduleForAnalysis(subject.CilMethodBody);
            }
        }
    }
}
