namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Provides a base implementation for the <see cref="IObjectAnalyzer"/> interface, providing a generic argument.
    /// </summary>
    /// <typeparam name="T">The type of objects to analyze.</typeparam>
    public abstract class ObjectAnalyzer<T> : IObjectAnalyzer
    {
        /// <inheritdoc />
        public bool CanAnalyze(AnalysisContext context, object subject) => subject is T;

        /// <inheritdoc />
        void IObjectAnalyzer.Analyze(AnalysisContext context, object subject) => Analyze(context, (T) subject);

        /// <summary>
        /// Analyzes the provided object.
        /// </summary>
        /// <param name="context">The analysis context in which the analyzer is situated in.</param>
        /// <param name="subject">The subject to analyze.</param>
        protected abstract void Analyze(AnalysisContext context, T subject);
    }
}
