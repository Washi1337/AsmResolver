namespace AsmResolver.Workspaces
{
    public class WorksapceProfile
    {
        /// <summary>
        /// Gets a collection of object analyzers that are used in this workspace.
        /// </summary>
        public AnalyzerRepository Analyzers
        {
            get;
        } = new();

    }
}
