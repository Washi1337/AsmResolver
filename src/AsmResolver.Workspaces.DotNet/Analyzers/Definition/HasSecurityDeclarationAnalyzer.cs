using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="IHasSecurityDeclaration"/> for its definitions
    /// </summary>
    public class HasSecurityDeclarationAnalyzer : ObjectAnalyzer<IHasSecurityDeclaration>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, IHasSecurityDeclaration subject)
        {
            if (context.HasAnalyzers(typeof(SecurityDeclaration)))
            {
                for (int i = 0; i < subject.SecurityDeclarations.Count; i++)
                    context.SchedulaForAnalysis(subject.SecurityDeclarations[i]);
            }
        }
    }
}
