using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="EventDefinition"/> analyzer.
    /// </summary>
    public class EventAnalyzer : ObjectAnalyzer<EventDefinition>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, EventDefinition subject)
        {
            // Schedule type for analysis.
            if (subject.EventType is not null && context.HasAnalyzers(subject.EventType.GetType()))
            {
                context.ScheduleForAnalysis(subject.EventType);
            }
        }
    }
}
