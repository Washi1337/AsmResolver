using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Provides a default implementation for an <see cref="IHasGenericParameters"/> analyzer.
    /// </summary>
    public class GenericParameterAnalyser : ObjectAnalyzer<IHasGenericParameters>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, IHasGenericParameters subject)
        {
            for (int i = 0; i < subject.GenericParameters.Count; i++)
            {
                var genericParameter = subject.GenericParameters[i];
                if (context.HasAnalyzers(typeof(GenericParameter)))
                {
                    context.SchedulaForAnalysis(genericParameter);
                }

                for (int j = 0; j < genericParameter.Constraints.Count; j++)
                {
                    var parameterConstraint = genericParameter.Constraints[j];
                    if (context.HasAnalyzers(typeof(GenericParameterConstraint)))
                    {
                        context.SchedulaForAnalysis(parameterConstraint);
                    }

                    if (parameterConstraint.Constraint is not null
                        && context.HasAnalyzers(parameterConstraint.Constraint.GetType()))
                    {
                        context.SchedulaForAnalysis(parameterConstraint.Constraint);
                    }
                }
            }
        }
    }
}
