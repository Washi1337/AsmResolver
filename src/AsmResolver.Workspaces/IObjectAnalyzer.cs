namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Provides a contract for analyzing objects in a workspace.
    /// </summary>
    public interface IObjectAnalyzer
    {
        /// <summary>
        /// Determines whether the provided object can be analyzed by this analyzer.
        /// </summary>
        /// <param name="context">The analysis context in which the analyzer is situated in.</param>
        /// <param name="subject">The subject to analyze.</param>
        /// <returns><c>true</c> if the object can be analyzed by this analyzer, <c>false</c> otherwise.</returns>
        bool CanAnalyze(AnalysisContext context, object subject);

        /// <summary>
        /// Analyzes the provided object.
        /// </summary>
        /// <param name="context">The analysis context in which the analyzer is situated in.</param>
        /// <param name="subject">The subject to analyze.</param>
        void Analyze(AnalysisContext context, object subject);
    }
}
