using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="CustomAttributeArgument"/> for its definitions
    /// </summary>
    public class CustomAttributeArgumentAnalyser : ObjectAnalyzer<CustomAttributeArgument>
    {
        public override void Analyze(AnalysisContext context, CustomAttributeArgument subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.ArgumentType);
            }
        }
    }
}
