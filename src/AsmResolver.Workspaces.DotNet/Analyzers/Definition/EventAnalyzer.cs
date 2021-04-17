using AsmResolver.DotNet;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="EventDefinition"/> analyzer.
    /// </summary>
    public class EventAnalyzer : ObjectAnalyzer<EventDefinition>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, EventDefinition subject)
        {
            // Schedule type for analysis.
            if (context.HasAnalyzers(subject.EventType.GetType()))
            {
                context.ScheduleForAnalysis(subject.EventType);
            }
        }
    }
}
