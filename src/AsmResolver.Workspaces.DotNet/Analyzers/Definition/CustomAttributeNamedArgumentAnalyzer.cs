using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="CustomAttributeNamedArgument"/> for its definitions
    /// </summary>
    public class CustomAttributeNamedArgumentAnalyzer : ObjectAnalyzer<CustomAttributeNamedArgument>
    {
        public override void Analyze(AnalysisContext context, CustomAttributeNamedArgument subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.ArgumentType);
            }

            if (subject.Argument is not null && context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.Argument.ArgumentType);
                for (int i = 0; i < subject.Argument.Elements.Count; i++)
                {
                    var element = subject.Argument.Elements[i];
                    if (element is not TypeSignature)
                        continue;
                    context.SchedulaForAnalysis(element);
                }
            }
        }
    }
}
