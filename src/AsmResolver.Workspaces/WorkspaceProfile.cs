namespace AsmResolver.Workspaces
{
    public class WorkspaceProfile
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
