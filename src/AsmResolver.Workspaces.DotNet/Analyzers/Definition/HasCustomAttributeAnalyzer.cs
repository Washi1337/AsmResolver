using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="IHasCustomAttribute"/> for its definitions
    /// </summary>
    public class HasCustomAttributeAnalyzer : ObjectAnalyzer<IHasCustomAttribute>
    {

        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, IHasCustomAttribute subject)
        {
            if (context.HasAnalyzers(typeof(CustomAttribute)))
            {
                for (int i = 0; i < subject.CustomAttributes.Count; i++)
                    context.SchedulaForAnalysis(subject.CustomAttributes[i]);
            }
        }
    }
}
