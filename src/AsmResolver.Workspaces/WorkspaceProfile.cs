namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Provides a base mechanism for storing and scheduling analyzers.
    /// </summary>
    public class WorksapceProfile
    {
        /// <summary>
        /// Gets a collection of object analyzers.
        /// </summary>
        public AnalyzerRepository Analyzers
        {
            get;
        } = new();

    }
}
