using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="TypeSpecification"/> analyzer.
    /// </summary>
    public class TypeSpecificationAnalyzer : ObjectAnalyzer<TypeSpecification>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, TypeSpecification subject)
        {
            if (context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.SchedulaForAnalysis(subject.Signature);
            }
        }
    }
}