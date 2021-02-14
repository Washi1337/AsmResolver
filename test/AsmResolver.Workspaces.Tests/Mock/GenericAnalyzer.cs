namespace AsmResolver.Workspaces.Tests.Mock
{
    public class GenericAnalyzer<T> : ObjectAnalyzer<T>
    {
        public static GenericAnalyzer<T> Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, T subject)
        {

        }
    }
}
