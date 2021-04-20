using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="IGenericArgumentsProvider"/> analyzer.
    /// </summary>
    public class GenericArgumentAnalyzer : ObjectAnalyzer<IGenericArgumentsProvider>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, IGenericArgumentsProvider subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                for (int i = 0; i < subject.TypeArguments.Count; i++)
                {
                    context.ScheduleForAnalysis(subject.TypeArguments[i]);
                }
            }
        }
    }
}
