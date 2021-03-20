using AsmResolver.DotNet;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="Parameter"/> analyzer.
    /// </summary>
    public class ParameterAnalyzer : ObjectAnalyzer<Parameter>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, Parameter subject)
        {
            // Schedule signature for analysis.
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.ParameterType);
            }

            if (context.HasAnalyzers(typeof(ParameterDefinition)))
            {
                context.SchedulaForAnalysis(subject.Definition);
            }
        }
    }
}
