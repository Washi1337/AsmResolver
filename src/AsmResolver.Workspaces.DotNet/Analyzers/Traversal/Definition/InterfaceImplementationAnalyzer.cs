using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="InterfaceImplementation"/> analyzer.
    /// </summary>
    public class InterfaceImplementationAnalyzer : ObjectAnalyzer<InterfaceImplementation>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, InterfaceImplementation subject)
        {
            if (subject.Interface is not null && context.HasAnalyzers(subject.Interface.GetType()))
            {
                context.ScheduleForAnalysis(subject.Interface);
            }
        }
    }
}
