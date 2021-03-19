using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="CustomAttributeNamedArgument"/> for its definitions
    /// </summary>
    public class CustomAttributeNamedArgumentAnalyser : ObjectAnalyzer<CustomAttributeNamedArgument>
    {
        public override void Analyze(AnalysisContext context, CustomAttributeNamedArgument subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.ArgumentType);
            }

            if (context.HasAnalyzers(typeof(CustomAttributeArgument)))
            {
                context.SchedulaForAnalysis(subject.Argument);
            }
        }
    }
}
