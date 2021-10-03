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
        protected override void Analyze(AnalysisContext context, TypeSpecification subject)
        {
            if (subject.Signature is not null && context.HasAnalyzers(typeof(TypeSignature)))
            {
                context.ScheduleForAnalysis(subject.Signature);
            }

            if (subject.DeclaringType is not null && context.HasAnalyzers(subject.DeclaringType.GetType()))
            {
                context.ScheduleForAnalysis(subject.DeclaringType);
            }
        }
    }
}
