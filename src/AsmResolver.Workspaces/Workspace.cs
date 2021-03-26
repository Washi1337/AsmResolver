namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Provides a base mechanism for indexing assemblies and their components.
    /// </summary>
    public abstract class Workspace
    {
        /// <summary>
        /// Gets a collection of object analyzers that are used in this workspace.
        /// </summary>
        public AnalyzerRepository Analyzers
        {
            get;
        } = new();

        /// <summary>
        /// Gets the index containing all analyzed objects.
        /// </summary>
        public WorkspaceIndex Index
        {
            get;
        } = new();

        /// <summary>
        /// Performs the analysis.
        /// </summary>
        /// <param name="context">The analysis context.</param>
        protected void Analyze(AnalysisContext context)
        {
            while (context.Agenda.Count > 0)
            {
                var nextSubject = context.Agenda.Dequeue();
                var analyzers = Analyzers.GetAnalyzers(nextSubject.GetType());
                foreach (var analyzer in analyzers)
                {
                    if (analyzer.CanAnalyze(context, nextSubject))
                        analyzer.Analyze(context, nextSubject);
                }
            }
        }
    }
}
