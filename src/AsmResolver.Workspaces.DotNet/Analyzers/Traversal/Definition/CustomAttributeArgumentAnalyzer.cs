using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Analyzes a <see cref="CustomAttributeArgument"/> for its definitions
    /// </summary>
    public class CustomAttributeArgumentAnalyzer : ObjectAnalyzer<CustomAttributeArgument>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, CustomAttributeArgument subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                for (int i = 0; i < subject.Elements.Count; i++)
                {
                    var element = subject.Elements[i];
                    if (element is not TypeSignature)
                        continue;
                    context.ScheduleForAnalysis(element);
                }
            }
        }
    }
}
